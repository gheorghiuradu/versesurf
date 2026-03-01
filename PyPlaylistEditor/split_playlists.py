import csv
import json
from pathlib import Path
from uuid import uuid4
from process_db_export import normalize_dir_name
from get_spoti_track import get_track_data

def short_uuid() -> str:
    u = uuid4()
    return str(u)[:13]

csv_path = Path("./VerseSurfDBExport20200909.csv")
if not csv_path.exists():
    print(f"CSV file not found: {csv_path}")
    exit(1)

snippet_edits_path = Path("./snippet_edits.json")
if not snippet_edits_path.exists():
    print(f"Snippet edits file not found: {snippet_edits_path}")
    exit(1)

with csv_path.open(newline="", encoding="utf-8") as csv_file:
    with snippet_edits_path.open(encoding="utf-8") as edits_file:
        snippet_edits = json.load(edits_file)
        reader = csv.DictReader(csv_file)
        fieldnames = reader.fieldnames

        if not fieldnames:
            print("CSV has no header row.")
            exit(1)

        current_playlist={
                            "Id": short_uuid(),
                            "Enabled": False,
                            "Name": "None",
                            "PictureUrl": "",
                            "Songs": [],
                        }
        for row in reader:
            playlist_name = (row.get("Name") or "").strip()
            if playlist_name and playlist_name != current_playlist["Name"]:

                if(len(current_playlist["Songs"]) > 0):
                    playlist_filename=f"{current_playlist["Id"]}_{normalize_dir_name(current_playlist['Name'].replace(' ', '_'))}.json"
                    first_track_data = get_track_data(current_playlist["Songs"][0]["SpotifyId"])
                    current_playlist["PictureUrl"] = first_track_data.get("cover_image_url", "")
                    with Path(f"./playlists/{playlist_filename}").open("w", encoding="utf-8") as playlist_file:
                        json.dump(current_playlist, playlist_file, ensure_ascii=False, indent=4)
                    print(f"Finished playlist: {current_playlist['Name']} with {len(current_playlist['Songs'])} songs")

                current_playlist={
                    "Id": short_uuid(),
                    "Enabled": True,
                    "Name": playlist_name,
                    "PictureUrl": "",
                    "Songs": [],
                }
                print(f"Playlist: {current_playlist['Name']}")

            track_id = (row.get("Songs.SpotifyId") or "").strip()
            if track_id:
                snippet_entry= next((edit for edit in snippet_edits if edit["spotifyId"] == track_id), None)
                if not snippet_entry:
                    print(f"No snippet edit found for track ID: {track_id}")
                    continue
                song_id = str(uuid4())
                track_data = get_track_data(track_id)
                song = {
                    "Id": song_id,
                    "SpotifyId": track_id,
                    "Title": row.get("Songs.Title", "").strip(),
                    "Artist": row.get("Songs.Artist", "").strip(),
                    "Snippet": snippet_entry.get("snippet", "").strip(),
                    "StartSecond": snippet_entry.get("startSecond", ""),
                    "EndSecond": snippet_entry.get("endSecond", ""),
                    "IsExplicit": row.get("Songs.IsExplicit", "").strip().lower() == "true",
                    "PreviewUrl": track_data.get("audio_preview_url", ""),
                    "Enabled": True
                }
                current_playlist["Songs"].append(song)
                current_playlist["Enabled"] = True

with Path(f"./playlists/{current_playlist['Id']}_{normalize_dir_name(current_playlist['Name'].replace(' ', '_'))}.json").open("w", encoding="utf-8") as playlist_file:
    json.dump(current_playlist, playlist_file, ensure_ascii=False, indent=4)
