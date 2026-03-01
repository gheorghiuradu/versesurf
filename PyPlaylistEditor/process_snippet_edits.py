import csv
import json
from pathlib import Path

csv_path = Path("./VerseSurfDBExport20200909.csv")
snippet_edits_path = Path("./snippet_edits.json")
if not csv_path.exists():
    print(f"CSV file not found: {csv_path}")
    exit(1)

if not snippet_edits_path.exists():
    print(f"Snippet edits file not found: {snippet_edits_path}")
    exit(1)

rows: list[dict[str, str]] = []
snippet_edits: list[any]

with csv_path.open(newline="", encoding="utf-8") as csv_file:
    with snippet_edits_path.open("r", encoding="utf-8") as edits_file:
        snippet_edits = json.load(edits_file)
        reader = csv.DictReader(csv_file)
        fieldnames = reader.fieldnames

        if not fieldnames:
            print("CSV has no header row.")
            exit(1)

        updated_rows = 0
        missing_snippet_corrections = 0

        for row in reader:
            playlist_name = (row.get("Name") or "").strip()
            track_id = (row.get("Songs.SpotifyId") or "").strip()

            if not track_id:
                rows.append(row)
                continue

            json_snippet_correction = next((
                edit for edit in snippet_edits
                if (edit.get("spotifyId") == track_id)
            ), None)

            if json_snippet_correction is None:
                missing_snippet_corrections += 1
                rows.append(row)
                continue

            try:
                corrected_snippet = json_snippet_correction.get("snippet")
                if isinstance(corrected_snippet, str) and len(corrected_snippet) > 0:
                    row["Songs.Snippet"] = corrected_snippet
                    row["Songs.StartSecond"] = json_snippet_correction.get("start_second", "")
                    row["Songs.EndSecond"] = json_snippet_correction.get("end_second", "")
                    updated_rows += 1
                else:
                    missing_snippet_corrections += 1
            except json.JSONDecodeError:
                missing_snippet_corrections += 1
                rows.append(row)
                continue

            rows.append(row)

temp_csv_path = csv_path.with_suffix(".tmp.csv")
with temp_csv_path.open("w", newline="", encoding="utf-8") as csv_file:
    writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(rows)

temp_csv_path.replace(csv_path)

print(f"Updated rows: {updated_rows}")
print(f"Rows without corrected_snippet: {missing_snippet_corrections}")
