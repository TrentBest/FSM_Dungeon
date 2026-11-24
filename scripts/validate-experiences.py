#!/usr/bin/env python3
"""Validate FSM_Dungeon experience manifests and originality declarations."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPERIENCES = ROOT / "docs" / "experiences"
REQUIRED = {"schemaVersion", "id", "title", "status", "originality", "domain", "integrations", "experience", "content", "firstVerticalSlice", "ciAi"}


def fail(path: Path, message: str) -> None:
    print(f"ERROR: {path.relative_to(ROOT)}: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> int:
    manifests = sorted(EXPERIENCES.glob("*.json"))
    if not manifests:
        print("ERROR: no experience manifests found", file=sys.stderr)
        return 1

    ids: set[str] = set()
    for path in manifests:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            fail(path, f"invalid JSON: {exc}")
        if not isinstance(data, dict):
            fail(path, "manifest root must be an object")
        missing = sorted(REQUIRED - data.keys())
        if missing:
            fail(path, f"missing fields: {', '.join(missing)}")

        identity = data["id"]
        if not isinstance(identity, str) or not identity.strip() or identity in ids:
            fail(path, "id must be a unique non-empty string")
        ids.add(identity)

        originality = data["originality"]
        if not isinstance(originality, dict):
            fail(path, "originality must be an object")
        for field in ("workshopOriginal", "thirdPartyAssets", "thirdPartyRuleText"):
            if type(originality.get(field)) is not bool:
                fail(path, f"originality.{field} must be boolean")
        if originality["workshopOriginal"] is not True:
            fail(path, "experience must declare Workshop-original authorship")
        if originality["thirdPartyAssets"] or originality["thirdPartyRuleText"]:
            fail(path, "third-party assets and copied rule text are prohibited")

        domain = data["domain"]
        if not isinstance(domain, dict) or domain.get("package") != "TheSingularityWorkshop.FSM_Dungeon":
            fail(path, "domain.package must reference the shared FSM_Dungeon package")
        if domain.get("capability") != "fsm-dungeon.underground-environment":
            fail(path, "domain.capability must reference the shared underground-environment capability")

        integrations = data["integrations"]
        if not isinstance(integrations, dict) or integrations.get("actorOntology", {}).get("owner") != "SURPS":
            fail(path, "actor ontology must remain owned by SURPS")
        if data["content"].get("noEngineSpecificContentEnums") is not True:
            fail(path, "experience content must not require engine-specific content enums")

        # Character creation is an integration contract owned by SURPS. Validate
        # the manifest boundary without making FSM_Dungeon responsible for the API.
        player_character = integrations.get("playerCharacter")
        if player_character is not None:
            if not isinstance(player_character, dict):
                fail(path, "integrations.playerCharacter must be an object")
            if player_character.get("owner") != "SURPS":
                fail(path, "player character schema and lifecycle must remain owned by SURPS")
            if player_character.get("entryPoint") != "Surps.Create.PlayerCharacter()":
                fail(path, "player character entry point must use the SURPS creation contract")
            if player_character.get("presentationOwner") != "GUI":
                fail(path, "player character intake presentation must be owned by GUI")
            persistence = player_character.get("persistence")
            if not isinstance(persistence, dict):
                fail(path, "integrations.playerCharacter.persistence must be an object")
            operations = persistence.get("operations")
            if not isinstance(operations, list) or not {"serialize", "deserialize"}.issubset(operations):
                fail(path, "player character persistence must support serialize and deserialize")
            if persistence.get("format") != "SURPS-owned-character-document":
                fail(path, "player character persistence must use the SURPS-owned document format")

            composition = data.get("composition")
            if not isinstance(composition, dict) or composition.get("loading") != "registered-book-and-experience-contributions":
                fail(path, "player character integration requires registered book/experience composition")
            settings = composition.get("settings")
            character_settings = settings.get("characterCreation") if isinstance(settings, dict) else None
            if not isinstance(character_settings, dict) or character_settings.get("enabled") is not True:
                fail(path, "composition.settings.characterCreation must be enabled")
            if not character_settings.get("profile"):
                fail(path, "character creation must name an experience profile")
            book_ids = composition.get("data", {}).get("registeredBooks")
            if not isinstance(book_ids, list) or not book_ids or any(not isinstance(item, str) or not item.strip() for item in book_ids):
                fail(path, "composition.data.registeredBooks must contain non-empty book IDs")
            quest_id = composition.get("data", {}).get("quest")
            if not isinstance(quest_id, str) or not quest_id.startswith("fsm-dungeon.quest."):
                fail(path, "composition.data.quest must reference a Dungeon quest ID")
            quest_doc = ROOT / "docs" / "quests" / (quest_id.rsplit(".", 1)[-1] + ".md")
            if not quest_doc.is_file():
                fail(path, f"quest document is missing: {quest_doc.relative_to(ROOT)}")
            if quest_id not in quest_doc.read_text(encoding="utf-8"):
                fail(path, f"quest document does not declare {quest_id}")

        slice_spec = data["firstVerticalSlice"]
        if not isinstance(slice_spec, dict) or not slice_spec.get("features"):
            fail(path, "firstVerticalSlice.features must be a non-empty list")
        if slice_spec.get("deterministicReplayRequired") is not True:
            fail(path, "deterministic replay is required")

        ci_ai = data["ciAi"]
        if not isinstance(ci_ai, dict) or ci_ai.get("requiresBuildAndTests") is not True:
            fail(path, "CiAi workloads must require build and test evidence")
        if ci_ai.get("requiresContentProvenance") is not True or ci_ai.get("requiresHumanApprovalForProtectedActions") is not True:
            fail(path, "CiAi workloads must preserve provenance and host approval gates")

        print(f"OK: {identity} ({data['status']})")

    print(f"Validated {len(manifests)} experience manifest(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
