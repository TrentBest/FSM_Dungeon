#!/usr/bin/env python3
"""Validate machine-readable FSM_Dungeon sourcebook descriptors."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BOOKS = ROOT / "docs" / "books"
REQUIRED = {
    "schemaVersion",
    "id",
    "title",
    "publisher",
    "repository",
    "source",
    "domain",
    "lod",
    "edition",
    "status",
    "entryNamespace",
    "ownership",
    "artwork",
}


def fail(path: Path, message: str) -> None:
    print(f"ERROR: {path.relative_to(ROOT)}: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> int:
    descriptors = sorted(BOOKS.glob("*.json"))
    if not descriptors:
        print("ERROR: no sourcebook descriptors found", file=sys.stderr)
        return 1

    seen_ids: set[str] = set()
    for path in descriptors:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            fail(path, f"invalid JSON: {exc}")

        missing = sorted(REQUIRED - data.keys())
        if missing:
            fail(path, f"missing required fields: {', '.join(missing)}")

        source_id = data["id"]
        if not isinstance(source_id, str) or not source_id.strip():
            fail(path, "id must be a non-empty string")
        if source_id in seen_ids:
            fail(path, f"duplicate sourcebook id: {source_id}")
        seen_ids.add(source_id)

        if not isinstance(data["lod"], int) or data["lod"] < 0:
            fail(path, "lod must be a non-negative integer")
        if not isinstance(data["edition"], str) or not data["edition"].strip():
            fail(path, "edition must be a non-empty string")

        source = data["source"]
        if not isinstance(source, str) or not source.strip():
            fail(path, "source must be a non-empty repository-relative path")
        source_path = (ROOT / source).resolve()
        if ROOT not in source_path.parents or not source_path.is_file():
            fail(path, f"source file does not exist inside repository: {source}")

        for field in ("publisher", "repository", "title", "domain", "entryNamespace"):
            if not isinstance(data[field], str) or not data[field].strip():
                fail(path, f"{field} must be a non-empty string")

        artwork = data["artwork"]
        if not isinstance(artwork, dict) or "cover" not in artwork or "status" not in artwork:
            fail(path, "artwork must explicitly declare cover and status")

        print(f"OK: {source_id} (LOD {data['lod']}, edition {data['edition']})")

    print(f"Validated {len(descriptors)} sourcebook descriptor(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
