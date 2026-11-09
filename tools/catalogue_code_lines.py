"""Rebuild the "LINES WRITTEN IN CODE" section at the end of DIALOGUES.txt.

Some dialogue is written straight into the scripts instead of dialogue assets. This finds those lines
in Assets/Scripts and lists them by file, in the order they appear, so they can be searched and
edited like the rest. Lines the game puts together at runtime come out as templates, with the parts
it fills in shown in {braces}: "{dish} for the {meal}th meal in a row."

Run from anywhere:  python tools/catalogue_code_lines.py
Everything above the section is left as it is.
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SCRIPTS = os.path.join(ROOT, 'Assets', 'Scripts')
CATALOGUE = os.path.join(ROOT, 'DIALOGUES.txt')
MARKER = 'LINES WRITTEN IN CODE'
HEADER = [
    MARKER,
    '=====================',
    'These lines live in the scripts rather than in dialogue assets. Edit them in the named file.',
    'Parts in {braces} are filled in by the game. Regenerate this section with:',
    '  python tools/catalogue_code_lines.py',
]

OPENERS = {'(': ')', '[': ']', '{': '}'}


def skip_string(src, i):
    """Index just past the string literal starting at src[i] ('"' or '$"')."""
    if src[i] == '$':
        i += 1
    i += 1
    while i < len(src):
        if src[i] == '\\':
            i += 2
            continue
        if src[i] == '"':
            return i + 1
        i += 1
    return i


def matching(src, i):
    """Index of the bracket closing the one at src[i]."""
    want, depth, j = [OPENERS[src[i]]], 0, i + 1
    while j < len(src):
        c = src[j]
        if c == '"' or (c == '$' and src[j + 1:j + 2] == '"'):
            j = skip_string(src, j)
            continue
        if c in OPENERS:
            want.append(OPENERS[c])
        elif want and c == want[-1]:
            want.pop()
            if not want:
                return j
        j += 1
    return len(src) - 1


def top_level(expr):
    """(index, char) for every character of expr outside strings and brackets."""
    out, depth, j = [], 0, 0
    while j < len(expr):
        c = expr[j]
        if c == '"' or (c == '$' and expr[j + 1:j + 2] == '"'):
            j = skip_string(expr, j)
            continue
        if c in '([{':
            depth += 1
        elif c in ')]}':
            depth -= 1
        elif depth == 0:
            out.append((j, c))
        j += 1
    return out


def expression_end(src, start):
    """Index where the expression starting at src[start] ends: a ; or , outside brackets, or the
    bracket that closes the call it's an argument of."""
    depth, j = 0, start
    while j < len(src):
        c = src[j]
        if c == '"' or (c == '$' and src[j + 1:j + 2] == '"'):
            j = skip_string(src, j)
            continue
        if c in '([{':
            depth += 1
        elif c in ')]}':
            if depth == 0:
                return j
            depth -= 1
        elif depth == 0 and c in ';,':
            return j
        j += 1
    return j


def split_top(expr, sep):
    parts, last = [], 0
    for j, c in top_level(expr):
        if c == sep:
            parts.append(expr[last:j])
            last = j + 1
    parts.append(expr[last:])
    return [p.strip() for p in parts if p.strip()]


def ternary(expr):
    """(condition, then, else) for a top-level ?: expression, or None."""
    marks = top_level(expr)
    for k, (j, c) in enumerate(marks):
        if c == '?' and expr[j + 1:j + 2] not in '?.' and expr[j - 1:j] != '?':
            nested = 0
            for j2, c2 in marks[k + 1:]:
                if c2 == '?' and expr[j2 + 1:j2 + 2] not in '?.':
                    nested += 1
                elif c2 == ':':
                    if nested == 0:
                        return expr[:j], expr[j + 1:j2].strip(), expr[j2 + 1:].strip()
                    nested -= 1
            return None
    return None


def unwrap(expr):
    expr = expr.strip()
    while expr.startswith('(') and matching(expr, 0) == len(expr) - 1:
        expr = expr[1:-1].strip()
    return expr


def literal(token):
    token = token.strip()
    if token.startswith('$"') and token.endswith('"'):
        return token[2:-1].replace('\\"', '"')
    if token.startswith('"') and token.endswith('"') and skip_string(token, 0) == len(token):
        return token[1:-1].replace('\\"', '"')
    return None


def placeholder(token):
    name = re.match(r'[A-Za-z_][\w.]*', token.strip())
    return '{' + (name.group(0).split('.')[-1] if name else '...') + '}'


def lines_from(expr):
    """Every line of text an expression can produce: one per branch of a ?:, with non-text parts as {name}."""
    expr = unwrap(expr)
    t = ternary(expr)
    if t:
        return lines_from(t[1]) + lines_from(t[2])
    pieces = split_top(expr, '+')
    if not any(literal(p) is not None for p in pieces):
        return []
    return [''.join(literal(p) if literal(p) is not None else placeholder(p) for p in pieces)]


def is_dialogue(text):
    """Leave out paths, keys, sound names, all-caps choice buttons and other non-dialogue strings."""
    if '/' in text or not re.search(r'[a-z]', text):
        return False
    return ' ' in text.strip() or bool(re.search(r'[.!?#]', text))


def found_in(src):
    found = []
    # Arrays of lines: ShowDialogueFromText, Dialogue.text, PickUpLines rows and the like
    for m in re.finditer(r'new\s*(?:string\s*)?\[\s*\]\s*\{', src):
        body_start = m.end() - 1
        body = src[body_start + 1:matching(src, body_start)]
        elements = split_top(body, ',')
        texts = [(t, m.start()) for e in elements for t in lines_from(e)]
        if all(' ' not in t and not re.search(r'[.!?#]', t) for t, _ in texts):
            continue  # a list of bare words (choice buttons, counting words), not lines
        found += texts
    # Single lines: prompts, constants and properties named *Line, and local variables named *line*
    singles = [
        r'HandScratch\.Prompt\s*\(',
        r'"ShowChoiceDialog"\s*,',
        r'const\s+string\s+\w*Line\w*\s*=',
        r'\b\w*Line\w*\s*(?:\([^)]*\))?\s*=>',
        r'\bstring\s+\w*[Ll]ine\w*\s*=',
    ]
    for pattern in singles:
        for m in re.finditer(pattern, src):
            found += [(t, m.start()) for t in lines_from(src[m.end():expression_end(src, m.end())])]
    return found


def build_section():
    blocks = []
    for dirpath, dirnames, files in os.walk(SCRIPTS):
        dirnames.sort()
        for f in sorted(files):
            if not f.endswith('.cs'):
                continue
            path = os.path.join(dirpath, f)
            src = open(path, encoding='utf-8-sig').read()
            seen, lines = set(), []
            for text, _ in sorted(found_in(src), key=lambda x: x[1]):
                if is_dialogue(text) and text not in seen:
                    seen.add(text)
                    lines.append(text)
            if lines:
                rel = os.path.relpath(path, ROOT).replace('\\', '/')
                blocks.append([f'[Code - {f[:-3]}]', f'FILE: {rel}'] + ['  ' + l for l in lines])
    return HEADER + [''] + [line for b in blocks for line in b + ['']]


def main():
    raw = open(CATALOGUE, 'rb').read().decode('utf-8')
    nl = '\r\n' if '\r\n' in raw else '\n'
    lines = raw.split(nl)
    if MARKER in lines:
        lines = lines[:lines.index(MARKER)]
    while lines and not lines[-1].strip():
        lines.pop()
    section = build_section()
    out = nl.join(lines + [''] + section).rstrip() + nl
    open(CATALOGUE, 'wb').write(out.encode('utf-8'))
    files = sum(1 for l in section if l.startswith('[Code - '))
    print(f'Updated {os.path.relpath(CATALOGUE, ROOT)}: {files} scripts')


if __name__ == '__main__':
    sys.exit(main())
