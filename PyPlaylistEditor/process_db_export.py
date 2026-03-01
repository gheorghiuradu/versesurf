"""Process VerseSurf CSV export and run track + karaoke preparation per entry.

For each CSV row, this script:
1) Calls get_spoti_track.py for Songs.SpotifyId with --output-dir ./playlists/{track_id}
2) Runs karaoke-gen using the downloaded preview and CSV artist/title metadata
"""

import argparse
import csv
import logging
import re
import subprocess
import sys
from pathlib import Path


logger = logging.getLogger(__name__)


def run_command(command: list[str]) -> subprocess.CompletedProcess[str]:
    """Run a subprocess command and return the completed process."""

    return subprocess.run(command, capture_output=True, text=True)


def normalize_dir_name(name: str) -> str:
    """Return a filesystem-safe directory name."""

    normalized = re.sub(r"[\\/:*?\"<>|]", "_", name).strip()
    normalized = re.sub(r"\s+", " ", normalized)
    return normalized or "Unknown Playlist"


def main() -> None:
    """Read CSV and process each entry by Spotify track id."""

    logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")

    parser = argparse.ArgumentParser(
        description="Run get_spoti_track.py and karaoke-gen for each row in a DB export CSV"
    )
    parser.add_argument(
        "--csv",
        default="VerseSurfDBExport20200909.csv",
        help="Path to VerseSurf DB export CSV",
    )
    parser.add_argument(
        "--playlists-dir",
        default="playlists",
        help="Base playlists directory (default: playlists)",
    )
    args = parser.parse_args()

    csv_path = Path(args.csv)
    playlists_dir = Path(args.playlists_dir)
    script_dir = Path(__file__).resolve().parent
    get_spoti_script = script_dir / "get_spoti_track.py"

    if not csv_path.exists():
        logger.error("CSV file not found: %s", csv_path)
        sys.exit(1)

    if not get_spoti_script.exists():
        logger.error("Script not found: %s", get_spoti_script)
        sys.exit(1)

    playlists_dir.mkdir(parents=True, exist_ok=True)

    total_rows = 0
    processed_rows = 0
    failed_rows = 0

    extensions_to_cleanup = [".mp3", ".lrc", ".txt", "*.wav"]

    with csv_path.open(newline="", encoding="utf-8") as csv_file:
        reader = csv.DictReader(csv_file)

        for row in reader:
            total_rows += 1

            playlist_name = (row.get("Name") or "").strip()
            track_id = (row.get("Songs.SpotifyId") or "").strip()
            artist = (row.get("Songs.Artist") or "").strip()
            title = (row.get("Songs.Title") or "").strip()

            if not track_id:
                logger.warning("Row %s missing Songs.SpotifyId. Skipping.", total_rows)
                failed_rows += 1
                continue

            playlist_dir = playlists_dir / normalize_dir_name(playlist_name)
            track_output_dir = playlist_dir / track_id
            preview_file = track_output_dir / f"{track_id}_preview.mp3"

            if not preview_file.exists():
                logger.info("[%s] Fetching Spotify assets for %s - %s (%s)", total_rows, artist, title, track_id)
                fetch_command = [
                    sys.executable,
                    str(get_spoti_script),
                    track_id,
                    "--output-dir",
                    str(track_output_dir),
                ]
                fetch_result = run_command(fetch_command)

                if fetch_result.returncode != 0:
                    logger.error("[%s] get_spoti_track.py failed for %s", total_rows, track_id)
                    if fetch_result.stderr:
                        logger.error(fetch_result.stderr.strip())
                    failed_rows += 1
                    continue

                if not preview_file.exists():
                    logger.warning(
                        "[%s] Preview file not found after fetch: %s. Skipping karaoke-gen.",
                        total_rows,
                        preview_file,
                    )
                    failed_rows += 1
                    continue

            lyrics_directory = track_output_dir / "lyrics"
            if not lyrics_directory.exists():
                karaoke_command = [
                    "karaoke-gen",
                    "--lyrics-only",
                    "--prep-only",
                    "--output_dir",
                    str(track_output_dir),
                    "--no_track_subfolders",
                    str(preview_file),
                    artist,
                    title,
                ]
                logger.info("[%s] Running karaoke-gen for %s", total_rows, track_id)
                karaoke_result = run_command(karaoke_command)

                if karaoke_result.returncode != 0:
                    logger.error("[%s] karaoke-gen failed for %s", total_rows, track_id)
                    if karaoke_result.stderr:
                        logger.error(karaoke_result.stderr.strip())
                    failed_rows += 1
                    continue

            for file_path in track_output_dir.rglob("*"):
                if file_path.is_file() and file_path.suffix.lower() in extensions_to_cleanup and file_path != preview_file:
                    file_path.unlink()
                    logger.info("[%s] Deleted: %s", total_rows, file_path)


            if lyrics_directory.exists() and lyrics_directory.is_dir():
                for file_path in lyrics_directory.glob("*.json"):
                    file_path.rename(track_output_dir / f"{track_id}_lyrics.json")
                lyrics_directory.rmdir()

            processed_rows += 1

    logger.info(
        "Done. Total rows: %s | Successful: %s | Failed/Skipped: %s",
        total_rows,
        processed_rows,
        failed_rows,
    )


if __name__ == "__main__":
    main()
