# <copyright file="check_locales.py" company="River-Mochi">
# Copyright (c) 2026 River-Mochi. All rights reserved.
# Licensed under the MIT License. You may not use this file except in compliance with this License.
# See LICENSE file in the project root for full license information.
# This notice and the MIT License notice must be kept with
# all copies or substantial portions of this code.
# ================= </copyright> ======================

# File: Shared Lib Scripts/check_locales.py
# Version: 0.4.2
# Checks C# Locale*.cs dictionaries against LocaleEN.cs.
# Supports either Locale/ or Localization/ folders.

from __future__ import annotations

import argparse
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import DefaultDict, Dict, Iterable, List, Optional, Tuple

DICT_START = re.compile(
    r"""
    (?:
        # Older explicit Dictionary constructor:
        # new Dictionary<string, string> { ... }
        # new Dictionary<string, string>() { ... }
        (?:return\s+)?
        new\s+
        (?:System\.Collections\.Generic\.)?
        Dictionary\s*<\s*string\s*,\s*string\s*>
        \s*(?:\(\s*\))?

      |

        # Newer target-typed constructor:
        # Dictionary<string, string> entries = new() { ... }
        (?:System\.Collections\.Generic\.)?
        Dictionary\s*<\s*string\s*,\s*string\s*>
        \s+[A-Za-z_]\w*
        \s*=\s*
        new\s*\(\s*\)
    )
    \s*\{
    """,
    re.IGNORECASE | re.VERBOSE,
)

# CS2 Options text uses <text> for green highlighted text.
# Match only complete markers on one displayed line. 
#  "value > 0" or "Options > Interface" are
# handled separately by marker_issues().
ANGLE_MARKER = re.compile(r"<([^<>\n]+)>")

LOCALE_FOLDER_NAMES = ("Locale", "Localization")

SKIP_DIRS = {
    ".git",
    ".idea",
    ".vs",
    ".vscode",
    "__pycache__",
    "bin",
    "build",
    "dist",
    "node_modules",
    "obj",
}


def is_escaped(text: str, index: int) -> bool:
    """True when the character at index has an odd number of preceding slashes."""
    slash_count = 0
    index -= 1

    while index >= 0 and text[index] == "\\":
        slash_count += 1
        index -= 1

    return slash_count % 2 == 1


def decode_normal_string(body: str) -> str:
    """Decode the C# escapes used in locale strings."""
    escapes = {
        "'": "'",
        '"': '"',
        "\\": "\\",
        "0": "\0",
        "a": "\a",
        "b": "\b",
        "f": "\f",
        "n": "\n",
        "r": "\r",
        "t": "\t",
        "v": "\v",
    }

    result: List[str] = []
    index = 0

    while index < len(body):
        char = body[index]
        if char != "\\" or index + 1 >= len(body):
            result.append(char)
            index += 1
            continue

        escape = body[index + 1]
        if escape in escapes:
            result.append(escapes[escape])
            index += 2
            continue

        if escape == "u" and index + 5 < len(body):
            digits = body[index + 2:index + 6]
            if re.fullmatch(r"[0-9A-Fa-f]{4}", digits):
                result.append(chr(int(digits, 16)))
                index += 6
                continue

        if escape == "U" and index + 9 < len(body):
            digits = body[index + 2:index + 10]
            if re.fullmatch(r"[0-9A-Fa-f]{8}", digits):
                result.append(chr(int(digits, 16)))
                index += 10
                continue

        if escape == "x":
            match = re.match(r"[0-9A-Fa-f]{1,4}", body[index + 2:])
            if match:
                result.append(chr(int(match.group(0), 16)))
                index += 2 + len(match.group(0))
                continue

        # Preserve an unknown escape so the checker does not silently alter it.
        result.extend(("\\", escape))
        index += 2

    return "".join(result)


def read_string(text: str, start: int) -> Optional[Tuple[str, str, int]]:
    """
    Read one C# normal or verbatim string.

    Returns (raw token, decoded value, next index), or None when start is not
    the beginning of a supported string.
    """
    verbatim = text.startswith('@"', start)
    quote_index = start + 1 if verbatim else start

    if quote_index >= len(text) or text[quote_index] != '"':
        return None

    index = quote_index + 1
    body_start = index

    if verbatim:
        decoded: List[str] = []
        segment_start = index

        while index < len(text):
            if text[index] != '"':
                index += 1
                continue

            if index + 1 < len(text) and text[index + 1] == '"':
                decoded.append(text[segment_start:index])
                decoded.append('"')
                index += 2
                segment_start = index
                continue

            decoded.append(text[segment_start:index])
            end = index + 1
            return text[start:end], "".join(decoded), end

        raise ValueError("Unterminated verbatim string")

    while index < len(text):
        if text[index] == '"' and not is_escaped(text, index):
            end = index + 1
            body = text[body_start:index]
            return text[start:end], decode_normal_string(body), end
        index += 1

    raise ValueError("Unterminated string")


def strip_comments(text: str) -> str:
    """Remove C# comments while preserving string literals."""
    result: List[str] = []
    index = 0

    while index < len(text):
        string = read_string(text, index)
        if string is not None:
            raw, _decoded, index = string
            result.append(raw)
            continue

        if text.startswith("//", index):
            newline = text.find("\n", index + 2)
            if newline < 0:
                break
            result.append("\n")
            index = newline + 1
            continue

        if text.startswith("/*", index):
            end = text.find("*/", index + 2)
            if end < 0:
                raise ValueError("Unterminated block comment")
            result.append("\n" * text[index:end + 2].count("\n"))
            index = end + 2
            continue

        result.append(text[index])
        index += 1

    return "".join(result)


def find_matching_brace(text: str, opening: int) -> int:
    """Find the closing brace matching text[opening]."""
    depth = 0
    index = opening

    while index < len(text):
        string = read_string(text, index)
        if string is not None:
            index = string[2]
            continue

        if text[index] == "{":
            depth += 1
        elif text[index] == "}":
            depth -= 1
            if depth == 0:
                return index

        index += 1

    raise ValueError("Unbalanced dictionary braces")


def dictionary_body(text: str) -> str:
    """Return the contents of the first Dictionary<string, string> initializer."""
    clean = strip_comments(text)
    match = DICT_START.search(clean)
    if match is None:
        raise ValueError("Dictionary<string, string> initializer not found")

    opening = clean.rfind("{", match.start(), match.end())
    closing = find_matching_brace(clean, opening)
    return clean[opening + 1:closing]


def dictionary_entries(body: str) -> Iterable[str]:
    """Yield each top-level { key, value } entry."""
    index = 0

    while index < len(body):
        string = read_string(body, index)
        if string is not None:
            index = string[2]
            continue

        if body[index] != "{":
            index += 1
            continue

        closing = find_matching_brace(body, index)
        yield body[index + 1:closing]
        index = closing + 1


def split_entry(entry: str) -> Tuple[str, str]:
    """Split one dictionary entry at its first top-level comma."""
    parentheses = 0
    brackets = 0
    braces = 0
    index = 0

    while index < len(entry):
        string = read_string(entry, index)
        if string is not None:
            index = string[2]
            continue

        char = entry[index]
        if char == "(":
            parentheses += 1
        elif char == ")":
            parentheses -= 1
        elif char == "[":
            brackets += 1
        elif char == "]":
            brackets -= 1
        elif char == "{":
            braces += 1
        elif char == "}":
            braces -= 1
        elif char == "," and parentheses == brackets == braces == 0:
            return entry[:index], entry[index + 1:]

        index += 1

    raise ValueError(f"Dictionary entry has no top-level comma: {entry.strip()[:80]}")


def normalize_key(expression: str) -> str:
    """Remove whitespace outside strings, preserving literal-key whitespace."""
    result: List[str] = []
    index = 0

    while index < len(expression):
        string = read_string(expression, index)
        if string is not None:
            raw, _decoded, index = string
            result.append(raw)
            continue

        if not expression[index].isspace():
            result.append(expression[index])
        index += 1

    return "".join(result)


def literal_text(expression: str) -> str:
    """Concatenate the normal and verbatim string literals in an expression."""
    result: List[str] = []
    index = 0

    while index < len(expression):
        string = read_string(expression, index)
        if string is None:
            index += 1
            continue

        _raw, decoded, index = string
        result.append(decoded)

    return "".join(result)


def placeholders(text: str) -> Counter[str]:
    """Return composite-format placeholder numbers; order does not matter."""
    unescaped = text.replace("{{", "").replace("}}", "")
    found = re.findall(r"\{(\d+)(?:,[^{}]+)?(?::[^{}]+)?\}", unescaped)
    return Counter(found)


def _angle_context(text: str, index: int, radius: int = 28) -> str:
    """Return a short one-line excerpt around an angle marker."""
    line_start = text.rfind("\n", 0, index) + 1
    line_end = text.find("\n", index)
    if line_end < 0:
        line_end = len(text)

    start = max(line_start, index - radius)
    end = min(line_end, index + radius + 1)
    excerpt = text[start:end].strip()
    if start > line_start:
        excerpt = "…" + excerpt
    if end < line_end:
        excerpt += "…"
    return excerpt


def _is_angle_operator_or_separator(text: str, index: int) -> bool:
    """
    True for a literal comparison/operator/separator rather than green markup.

    Supported examples:
      value > 0
      5 < Maximum
      value >= threshold
      value<maximum
      Options > Interface > Text Scaling
      left -> right
    """
    char = text[index]
    left = text[index - 1] if index > 0 else ""
    right = text[index + 1] if index + 1 < len(text) else ""

    # Normal spaced comparisons and UI breadcrumb separators.
    if left and right and left.isspace() and right.isspace():
        return True

    # Compact comparison forms such as x>0, 5<max, x>=0, or x<=max.
    if left and right and left.isalnum() and right.isalnum():
        return True
    if char == ">" and right and right.isdigit():
        return True
    if char == "<" and left and left.isdigit():
        return True
    if (left and left in "<>=") or (right and right in "<>="):
        return True

    # Text arrows are not green markup.
    if left == "-" or right == "-":
        return True

    return False


def marker_issues(text: str) -> List[str]:
    """Check bold, format placeholders, and CS2 green-text markers."""
    issues: List[str] = []

    if text.count("**") % 2:
        issues.append("unbalanced ** marker")

    brace_text = text.replace("{{", "").replace("}}", "")
    if brace_text.count("{") != brace_text.count("}"):
        issues.append(
            f"unbalanced braces: {{={brace_text.count('{')} }}={brace_text.count('}')}"
        )

    # Mask valid <text> markers first. This correctly accepts markers whose
    # contents end in a number, such as <Mod default = 40,000>.
    remaining = list(text)
    for match in ANGLE_MARKER.finditer(text):
        content = match.group(1)

        # CS2 markup is written without padding directly inside < and >.
        # Leave malformed forms such as < text> or <text > for the scan below.
        if content[0].isspace() or content[-1].isspace():
            continue

        for index in range(match.start(), match.end()):
            remaining[index] = " "

    residual = "".join(remaining)
    unmatched_left: List[int] = []
    unmatched_right: List[int] = []

    for index, char in enumerate(residual):
        if char not in "<>":
            continue
        if _is_angle_operator_or_separator(residual, index):
            continue

        if char == "<":
            unmatched_left.append(index)
        else:
            unmatched_right.append(index)

    for index in unmatched_left:
        issues.append(
            f"unclosed < highlight marker near: {_angle_context(text, index)!r}"
        )
    for index in unmatched_right:
        issues.append(
            f"unopened > highlight marker near: {_angle_context(text, index)!r}"
        )

    return issues


def load_locale(path: Path) -> Tuple[Dict[str, str], List[str], Dict[str, str]]:
    """Load normalized key/value data plus display names for reporting."""
    body = dictionary_body(path.read_text(encoding="utf-8-sig"))
    values: Dict[str, str] = {}
    keys: List[str] = []
    display: Dict[str, str] = {}

    for entry in dictionary_entries(body):
        key_expression, value_expression = split_entry(entry)
        key = normalize_key(key_expression)
        if not key:
            continue

        keys.append(key)
        values[key] = literal_text(value_expression)
        display.setdefault(key, " ".join(key_expression.split()))

    return values, keys, display


def _is_skipped_path(path: Path) -> bool:
    """True when path is inside a skipped repo/build/dependency folder."""
    return any(part in SKIP_DIRS for part in path.parts)


def find_localization_dir(repo: Path, baseline: str) -> Path:
    """Find the locale folder beneath repo. Supports both Locale/ and Localization/."""
    for folder_name in LOCALE_FOLDER_NAMES:
        direct = repo / folder_name
        if (direct / baseline).is_file():
            return direct

    matches = [
        path.parent
        for path in repo.rglob(baseline)
        if not _is_skipped_path(path)
        and path.parent.name in LOCALE_FOLDER_NAMES
    ]

    unique = sorted(set(matches))
    if not unique:
        expected = " or ".join(
            f"{folder_name}/{baseline}" for folder_name in LOCALE_FOLDER_NAMES
        )
        raise FileNotFoundError(
            f"Could not find {expected} under {repo}"
        )

    if len(unique) > 1:
        locations = "\n  ".join(str(path) for path in unique)
        raise ValueError(
            "Multiple locale folders found. Use --localization:\n  "
            + locations
        )

    return unique[0]


def check_locale(
    path: Path,
    baseline_values: Dict[str, str],
    baseline_keys: set[str],
) -> Tuple[bool, List[str]]:
    """Check one locale and return (has_problem, report lines)."""
    values, raw_keys, display = load_locale(path)
    key_set = set(values)

    duplicate_keys = sorted(
        display.get(key, key)
        for key, count in Counter(raw_keys).items()
        if count > 1
    )
    missing = sorted(baseline_keys - key_set)
    extra = sorted(key_set - baseline_keys)
    warnings: DefaultDict[str, List[str]] = defaultdict(list)

    for key, value in values.items():
        for issue in marker_issues(value):
            warnings[display.get(key, key)].append(issue)

        baseline_value = baseline_values.get(key, "")
        if baseline_value and value:
            expected = placeholders(baseline_value)
            actual = placeholders(value)
            if expected != actual:
                warnings[display.get(key, key)].append(
                    f"placeholders differ: EN={dict(expected)} locale={dict(actual)}"
                )

    report: List[str] = [f"{path.name}: {len(values)} keys"]

    if duplicate_keys:
        report.append("  Duplicate keys:")
        report.extend(f"    - {item}" for item in duplicate_keys)
    if missing:
        report.append("  Missing keys:")
        report.extend(f"    - {item}" for item in missing)
    if extra:
        report.append("  Extra keys:")
        report.extend(f"    - {item}" for item in extra)
    if warnings:
        report.append("  Value warnings:")
        for key in sorted(warnings):
            report.append(f"    - {key}")
            report.extend(f"      {warning}" for warning in warnings[key])

    has_problem = bool(duplicate_keys or missing or extra or warnings)
    return has_problem, report


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Check C# Locale*.cs dictionaries against LocaleEN.cs."
    )
    parser.add_argument(
        "--repo",
        type=Path,
        default=Path.cwd(),
        help="Repository root to scan (default: current directory).",
    )
    parser.add_argument(
        "--localization",
        type=Path,
        help="Exact locale directory, such as Locale or Localization; overrides --repo discovery.",
    )
    parser.add_argument(
        "--baseline",
        default="LocaleEN.cs",
        help="Baseline filename (default: LocaleEN.cs).",
    )
    parser.add_argument(
        "--pattern",
        default="Locale*.cs",
        help="Locale filename pattern (default: Locale*.cs).",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Print passing locale files too.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    try:
        repo = args.repo.resolve()
        localization = (
            args.localization.resolve()
            if args.localization is not None
            else find_localization_dir(repo, args.baseline)
        )
        baseline_path = localization / args.baseline
        if not baseline_path.is_file():
            raise FileNotFoundError(f"Baseline not found: {baseline_path}")

        baseline_values, _baseline_raw, _baseline_display = load_locale(
            baseline_path
        )
        baseline_keys = set(baseline_values)
        locale_files = sorted(localization.glob(args.pattern))
        if not locale_files:
            raise FileNotFoundError(
                f"No files matched {localization / args.pattern}"
            )

        print(f"Localization: {localization}")
        print(f"Baseline: {args.baseline} ({len(baseline_keys)} keys)")

        any_problem = False
        for path in locale_files:
            try:
                has_problem, report = check_locale(
                    path,
                    baseline_values,
                    baseline_keys,
                )
            except Exception as error:
                has_problem = True
                report = [f"{path.name}: ERROR: {error}"]

            any_problem |= has_problem
            if has_problem or args.verbose:
                print()
                print("\n".join(report))

        if any_problem:
            print("\nLocale check FAILED.")
            return 1

        print("All locale checks GOOD.")
        return 0

    except (FileNotFoundError, OSError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
