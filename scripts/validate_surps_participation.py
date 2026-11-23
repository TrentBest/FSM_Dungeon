#!/usr/bin/env python3
"""Validate FSM_Dungeon's SURPS sourcebook participation declaration.

This is a local consistency gate. It does not accept ownership claims or enroll
the repository; those decisions remain with the independent SURPS authority.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / ".surps" / "participation.json"
REQUIRED_DESCRIPTOR_FIELDS = {
    "schemaVersion", "id", "category", "lod", "title", "version",
    "cover", "lodBadge", "publisher", "publisherMoniker",
}


def fail(errors: list[str], message: str) -> None:
    errors.append(message)


def read_json(path: Path, errors: list[str]) -> dict | None:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as exc:
        fail(errors, f"{path.relative_to(ROOT)}: cannot read JSON: {exc}")
        return None
    if not isinstance(value, dict):
        fail(errors, f"{path.relative_to(ROOT)}: expected a JSON object")
        return None
    return value


def safe_file(value: object, label: str, errors: list[str]) -> Path | None:
    if not isinstance(value, str) or not value.strip():
        fail(errors, f"{label}: expected a non-empty repository-relative path")
        return None
    candidate = (ROOT / value).resolve()
    if ROOT.resolve() not in candidate.parents:
        fail(errors, f"{label}: path escapes the repository")
        return None
    if not candidate.is_file():
        fail(errors, f"{label}: file does not exist: {value}")
        return None
    return candidate


def main() -> int:
    errors: list[str] = []
    manifest = read_json(MANIFEST, errors)
    if manifest is None:
        return 1

    if manifest.get("schemaVersion") != 1:
        fail(errors, "participation.json: schemaVersion must be 1")
    if manifest.get("repository") != "TrentBest/FSM_Dungeon":
        fail(errors, "participation.json: repository identity must be TrentBest/FSM_Dungeon")
    if manifest.get("participationStatus") not in {
        "candidate-not-enrolled", "integrated-not-enrolled", "active"
    }:
        fail(errors, "participation.json: invalid participationStatus")

    books = manifest.get("books")
    if not isinstance(books, list) or not books:
        fail(errors, "participation.json: books must be a non-empty array")
        books = []

    seen_ids: set[str] = set()
    seen_covers: set[str] = set()
    checked = 0
    for index, entry in enumerate(books):
        label = f"participation.json: books[{index}]"
        if not isinstance(entry, dict):
            fail(errors, f"{label}: expected an object")
            continue

        book_id = entry.get("id")
        if not isinstance(book_id, str) or not book_id.strip() or book_id in seen_ids:
            fail(errors, f"{label}.id: must be a unique non-empty string")
        else:
            seen_ids.add(book_id)

        for field in ("title", "category", "version", "status"):
            if not isinstance(entry.get(field), str) or not entry[field].strip():
                fail(errors, f"{label}.{field}: must be a non-empty string")
        lod = entry.get("lod")
        if not isinstance(lod, int) or isinstance(lod, bool) or lod < 0:
            fail(errors, f"{label}.lod: must be a non-negative integer")
        if entry.get("status") not in {"proposed", "draft", "published"}:
            fail(errors, f"{label}.status: must be proposed, draft, or published")

        source_path = safe_file(entry.get("bookPath"), f"{label}.bookPath", errors)
        descriptor_path = safe_file(entry.get("descriptorPath"), f"{label}.descriptorPath", errors)
        cover_path = safe_file(entry.get("coverPath"), f"{label}.coverPath", errors)
        if isinstance(entry.get("coverPath"), str):
            if entry["coverPath"] in seen_covers:
                fail(errors, f"{label}.coverPath: each sourcebook needs a distinct cover")
            seen_covers.add(entry["coverPath"])

        if descriptor_path is None:
            continue
        descriptor = read_json(descriptor_path, errors)
        if descriptor is None:
            continue
        checked += 1

        missing = REQUIRED_DESCRIPTOR_FIELDS - descriptor.keys()
        for field in sorted(missing):
            fail(errors, f"{entry['descriptorPath']}: missing required field {field!r}")
        if missing:
            continue

        for field in ("id", "title", "category", "lod", "version"):
            if descriptor.get(field) != entry.get(field):
                fail(errors, f"{entry['descriptorPath']}: {field} differs from manifest")
        if descriptor.get("schemaVersion") != 1:
            fail(errors, f"{entry['descriptorPath']}: schemaVersion must be 1")
        if descriptor.get("source") != entry.get("bookPath"):
            fail(errors, f"{entry['descriptorPath']}: source must match bookPath")
        if descriptor.get("cover") != entry.get("coverPath"):
            fail(errors, f"{entry['descriptorPath']}: cover must match coverPath")

        badge = descriptor.get("lodBadge")
        expected_badge = (
            "https://raw.githubusercontent.com/TrentBest/"
            "TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/"
            f"development/docs/assets/rulebook-lod-{entry.get('lod')}.svg"
        )
        if not isinstance(badge, str) or badge != expected_badge:
            fail(errors, f"{entry['descriptorPath']}: lodBadge must use the canonical SURPS LOD {entry.get('lod')} asset URL")
        artwork = descriptor.get("artwork")
        if isinstance(artwork, dict) and artwork.get("lodBadge") not in (None, badge):
            fail(errors, f"{entry['descriptorPath']}: artwork.lodBadge differs from canonical lodBadge")

        for path, field in ((cover_path, "cover"),):
            if path is not None and path.suffix.lower() != ".svg":
                fail(errors, f"{label}.{field}: sourcebook cover must be an SVG asset")
        if isinstance(descriptor.get("publisherMoniker"), str) and not descriptor["publisherMoniker"].startswith("https://"):
            fail(errors, f"{entry['descriptorPath']}: publisherMoniker must use HTTPS")

        if source_path is not None:
            source = source_path.read_text(encoding="utf-8")
            heading = re.search(r"^# (.+)$", source, re.MULTILINE)
            if not heading or heading.group(1).strip() != entry.get("title"):
                fail(errors, f"{entry['bookPath']}: first-level heading must match the canonical title")
            if f"`{book_id}`" not in source:
                fail(errors, f"{entry['bookPath']}: stable sourcebook ID is missing")
            if f"**Edition:** {entry.get('version')}" not in source:
                fail(errors, f"{entry['bookPath']}: edition must match the manifest version")

    if errors:
        print("FSM_Dungeon SURPS participation validation FAILED:")
        for error in errors:
            print(f" - {error}")
        return 1

    print(f"FSM_Dungeon SURPS participation validation passed: {len(books)} books, {checked} descriptors.")
    print("Source identity, descriptor metadata, distinct covers, LOD badges, and local paths agree.")
    print(f"Participation remains {manifest['participationStatus']}; this check does not approve ownership claims.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
