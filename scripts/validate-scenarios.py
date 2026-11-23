#!/usr/bin/env python3
"""Validate data-driven FSM_Dungeon scenario fixtures."""

from __future__ import annotations

import json
import math
import re
import sys
from collections import deque
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENARIOS = ROOT / "samples" / "Dungeon.Proof" / "Scenarios"


def fail(path: Path, message: str) -> None:
    print(f"ERROR: {path.relative_to(ROOT)}: {message}", file=sys.stderr)
    raise SystemExit(1)


def unique_ids(path: Path, records: object, label: str) -> set[str]:
    if not isinstance(records, list) or not records:
        fail(path, f"{label} must be a non-empty list")
    result: set[str] = set()
    for item in records:
        if not isinstance(item, dict):
            fail(path, f"{label} entries must be objects")
        identity = item.get("id")
        if not isinstance(identity, str) or not identity.strip() or identity in result:
            fail(path, f"{label} identifiers must be unique non-empty strings")
        result.add(identity)
    return result


def finite_number(value: object) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(value)


def main() -> int:
    files = sorted(SCENARIOS.glob("*.json"))
    if not files:
        print("ERROR: no scenario fixtures found", file=sys.stderr)
        return 1

    for path in files:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            fail(path, f"invalid JSON: {exc}")
        if not isinstance(data, dict):
            fail(path, "root must be an object")

        for field in ("schemaVersion", "id", "title", "world", "presentation", "regions",
                      "features", "connections", "actorReferences", "information",
                      "objectives", "outcomes", "replay"):
            if field not in data:
                fail(path, f"missing required field '{field}'")

        world = data["world"]
        if not isinstance(world, dict) or not isinstance(world.get("seed"), int):
            fail(path, "world.seed must be an integer")
        if world.get("authoritativeModel") != "three-dimensional-spatial-topology":
            fail(path, "the authoritative model must remain three-dimensional spatial topology")

        presentation = data["presentation"]
        if not isinstance(presentation, dict) or presentation.get("projection") != "orthogonal-grid":
            fail(path, "presentation must declare the orthogonal-grid projection")
        if not isinstance(presentation.get("cellSize"), (int, float)) or presentation["cellSize"] <= 0:
            fail(path, "presentation.cellSize must be positive")
        if "not replace world geometry" not in presentation.get("note", ""):
            fail(path, "presentation must state that the grid does not replace world geometry")

        regions = data["regions"]
        region_ids = unique_ids(path, regions, "regions")
        cells: set[tuple[int, int]] = set()
        for region in regions:
            origin, extent, grid = region.get("origin"), region.get("extent"), region.get("grid")
            if not isinstance(origin, dict) or not all(finite_number(origin.get(k)) for k in ("x", "y", "z")):
                fail(path, f"{region['id']} must have finite 3D origin coordinates")
            if not isinstance(extent, dict) or not all(finite_number(extent.get(k)) and extent[k] > 0 for k in ("width", "height", "depth")):
                fail(path, f"{region['id']} must have positive finite 3D extents")
            if not finite_number(region.get("floorElevation")) or not region.get("formationReason"):
                fail(path, f"{region['id']} must retain elevation and formation provenance")
            if not isinstance(grid, dict) or not all(isinstance(grid.get(k), int) for k in ("x", "y")):
                fail(path, f"{region['id']} must have integer presentation grid coordinates")
            cell = (grid["x"], grid["y"])
            if cell in cells:
                fail(path, "presentation grid cells must be unique")
            cells.add(cell)

        feature_ids = unique_ids(path, data["features"], "features")
        for feature in data["features"]:
            if feature.get("regionId") is not None and feature["regionId"] not in region_ids:
                fail(path, f"feature {feature['id']} references an unknown region")
            if not feature.get("kind") or not feature.get("formationReason"):
                fail(path, f"feature {feature['id']} must retain kind and provenance")

        connection_ids = unique_ids(path, data["connections"], "connections")
        graph = {identity: set() for identity in region_ids}
        for connection in data["connections"]:
            left, right = connection.get("fromRegionId"), connection.get("toRegionId")
            if left not in region_ids or right not in region_ids or left == right:
                fail(path, f"connection {connection['id']} must reference two distinct regions")
            required = connection.get("requiredFeatureId")
            if required is not None and required not in feature_ids:
                fail(path, f"connection {connection['id']} references an unknown crossing feature")
            graph[left].add(right)
            graph[right].add(left)
        reached = set()
        pending = deque([next(iter(region_ids))])
        while pending:
            current = pending.popleft()
            if current in reached:
                continue
            reached.add(current)
            pending.extend(graph[current] - reached)
        if reached != region_ids:
            fail(path, "all regions must be spatially connected, even if a crossing feature is unavailable")

        actor_ids = unique_ids(path, data["actorReferences"], "actorReferences")
        for actor in data["actorReferences"]:
            if actor.get("provider") != "SURPS" or actor.get("initialRegionId") not in region_ids:
                fail(path, f"actor reference {actor['id']} must reference SURPS and a valid region")
            if not actor.get("externalIdentity"):
                fail(path, f"actor reference {actor['id']} needs an opaque external identity")

        information = data["information"]
        if information.get("authoritativeWorldIsOmniscient") is not True or information.get("discoveryIsObserverScoped") is not True:
            fail(path, "authoritative truth and observer-scoped discovery must remain separate")
        if any(identity not in feature_ids for identity in information.get("hiddenFeatureIds", [])):
            fail(path, "hiddenFeatureIds must reference known features")

        objective_ids = unique_ids(path, data["objectives"], "objectives")
        cache_id = data.get("cache", {}).get("id")
        if not cache_id or data["cache"].get("regionId") not in region_ids:
            fail(path, "cache must have an identity and a valid region")
        for objective in data["objectives"]:
            condition = objective.get("completionCondition", "")
            cache_condition = f"cache.recovered:{cache_id}"
            if condition == cache_condition:
                continue
            if condition.startswith("connection.traversable:") and condition.split(":", 1)[1] in connection_ids:
                continue
            fail(path, f"objective {objective['id']} has an unsupported or unresolved completion condition")
        if not data["outcomes"]:
            fail(path, "at least one explicit outcome is required")
        outcome_ids = unique_ids(path, data["outcomes"], "outcomes")
        for outcome in data["outcomes"]:
            if not outcome.get("when") or not outcome.get("result"):
                fail(path, f"outcome {outcome['id']} needs conditions and a result")
            for condition in outcome["when"]:
                prefix = "objective.complete:"
                if not condition.startswith(prefix) or condition[len(prefix):] not in objective_ids:
                    fail(path, f"outcome {outcome['id']} condition must reference a declared objective")

        replay = data["replay"]
        if replay.get("deterministic") is not True or not isinstance(replay.get("commandLog"), list):
            fail(path, "replay must declare deterministic execution and a command log")
        if not isinstance(replay.get("expectedStateDigest"), str) or not re.fullmatch(r"[0-9a-f]{64}", replay["expectedStateDigest"]):
            fail(path, "replay.expectedStateDigest must be a verified lowercase SHA-256 digest")
        previous = 0
        for command in replay["commandLog"]:
            sequence = command.get("sequence")
            if not isinstance(sequence, int) or sequence <= previous:
                fail(path, "command sequence numbers must be strictly increasing")
            previous = sequence
            if command.get("actorReferenceId") not in actor_ids:
                fail(path, f"command {sequence} references an unknown actor")
            target = command.get("targetRegionId") or command.get("targetId")
            known_targets = region_ids | feature_ids | objective_ids | outcome_ids
            cache_id = data.get("cache", {}).get("id")
            if cache_id:
                known_targets.add(cache_id)
            if target not in known_targets:
                fail(path, f"command {sequence} references an unknown target")
        print(f"OK: {data['id']} ({len(region_ids)} regions, {len(connection_ids)} connections)")

    print(f"Validated {len(files)} scenario fixture(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
