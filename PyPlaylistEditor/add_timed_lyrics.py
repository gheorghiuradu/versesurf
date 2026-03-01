import csv
import json
from pathlib import Path
from process_db_export import normalize_dir_name

csv_path = Path("./VerseSurfDBExport20200909.csv")
if not csv_path.exists():
    print(f"CSV file not found: {csv_path}")
    exit(1)

rows: list[dict[str, str]] = []

with csv_path.open(newline="", encoding="utf-8") as csv_file:
    reader = csv.DictReader(csv_file)
    fieldnames = reader.fieldnames

    if not fieldnames:
        print("CSV has no header row.")
        exit(1)

    updated_rows = 0
    missing_lyrics_files = 0
    missing_corrected_segments = 0

    for row in reader:
        playlist_name = (row.get("Name") or "").strip()
        track_id = (row.get("Songs.SpotifyId") or "").strip()

        if not track_id:
            rows.append(row)
            continue

        normalized_playlist_name = normalize_dir_name(playlist_name)

        lyrics_file = Path(f"./playlists/{normalized_playlist_name}/{track_id}/{track_id}_lyrics.json")
        if not lyrics_file.exists():
            legacy_lyrics_file = Path(f"./playlists/{normalized_playlist_name}/{track_id}_lyrics.json")
            if legacy_lyrics_file.exists():
                lyrics_file = legacy_lyrics_file
            else:
                missing_lyrics_files += 1
                rows.append(row)
                continue

        try:
            with lyrics_file.open("r", encoding="utf-8") as lyrics_handle:
                lyrics_payload = json.load(lyrics_handle)
        except json.JSONDecodeError:
            missing_corrected_segments += 1
            rows.append(row)
            continue

        corrected_segments = lyrics_payload.get("corrected_segments")
        if isinstance(corrected_segments, list):
            row["Songs.TimedLyrics"] = json.dumps(corrected_segments, ensure_ascii=False)
            updated_rows += 1
        else:
            missing_corrected_segments += 1

        rows.append(row)

temp_csv_path = csv_path.with_suffix(".tmp.csv")
with temp_csv_path.open("w", newline="", encoding="utf-8") as csv_file:
    writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(rows)

temp_csv_path.replace(csv_path)

print(f"Updated rows: {updated_rows}")
print(f"Rows without lyrics file: {missing_lyrics_files}")
print(f"Rows without corrected_segments: {missing_corrected_segments}")
