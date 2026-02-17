"""Fetch Spotify track metadata and download preview assets.

Usage:
    python get_spoti_track.py <track_id>
"""

import requests
import re
import json
import html
import argparse
import logging
from pathlib import Path
from typing import TypedDict


logger = logging.getLogger(__name__)


class TrackData(TypedDict):
    """Structured track metadata extracted from Spotify embed page JSON."""

    title: str
    artists: list[str]
    cover_image_url: str | None
    audio_preview_url: str | None


def _extract_cover_image_url(entity: dict) -> str | None:
    """Return preferred cover image URL if present, otherwise None."""

    visual_identity = entity.get("visualIdentity", {})
    images = visual_identity.get("image", [])
    if not images:
        return None

    if len(images) > 2 and isinstance(images[2], dict):
        return images[2].get("url")

    if isinstance(images[-1], dict):
        return images[-1].get("url")

    return None


def get_track_data(track_id: str) -> TrackData | None:
    """Return selected track metadata for a Spotify track ID.

    Args:
        track_id: Spotify track ID.

    Returns:
        A dictionary with title, artists, cover image URL, and audio preview URL,
        or None when expected embedded JSON is not found.

    Raises:
        requests.exceptions.RequestException: If network requests fail.
        json.JSONDecodeError: If embedded JSON cannot be parsed.
        KeyError, IndexError, TypeError: If expected fields are missing.
    """

    embed_url = f"https://open.spotify.com/embed/track/{track_id}"
    response = requests.get(embed_url)
    response.raise_for_status()

    html_content = response.text
    json_next_data_match = re.search(
        r'<script\s+id="__NEXT_DATA__"\s+type="application/json">(.*?)</script>',
        html_content,
        re.DOTALL,
    )

    if not json_next_data_match:
        return None

    json_raw = html.unescape(json_next_data_match.group(1)).strip()
    json_next_data = json.loads(json_raw)
    entity = json_next_data["props"]["pageProps"]["state"]["data"]["entity"]

    artists = entity["artists"]
    artist_names = [artist["name"] for artist in artists]
    audio_preview = entity.get("audioPreview", {})

    return {
        "title": entity["name"],
        "artists": artist_names,
        "cover_image_url": _extract_cover_image_url(entity),
        "audio_preview_url": audio_preview.get("url"),
    }


def download_file(url: str, output_path: str) -> None:
    """Download a file from a URL and save it to disk.

    Args:
        url: Source URL to download.
        output_path: Destination file path.

    Raises:
        requests.exceptions.RequestException: If download fails.
        OSError: If writing to disk fails.
    """

    file_response = requests.get(url)
    file_response.raise_for_status()
    with open(output_path, "wb") as output_file:
        output_file.write(file_response.content)


def main() -> None:
    """Parse CLI args, fetch track metadata, print values, and download assets."""

    logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")

    parser = argparse.ArgumentParser(description="Fetch Spotify track metadata and media previews")
    parser.add_argument("track_id", help="Spotify track ID (e.g. 6SkGfPa77E4giShVbk9N6R)")
    parser.add_argument(
        "-o",
        "--output-dir",
        default=".",
        help="Directory where downloaded files will be saved (default: current directory)",
    )
    args = parser.parse_args()

    track_id = args.track_id
    output_dir = Path(args.output_dir)

    try:
        output_dir.mkdir(parents=True, exist_ok=True)

        track_data = get_track_data(track_id)
        if not track_data:
            print("No __NEXT_DATA__ script tag found in the page.")
            return

        print("Artists:", " & ".join(track_data["artists"]))
        print("Title:", track_data["title"])
        print("Cover Image URL:", track_data["cover_image_url"])
        print("Audio Preview URL:", track_data["audio_preview_url"])

        if track_data["audio_preview_url"]:
            preview_path = output_dir / f"{track_id}_preview.mp3"
            download_file(track_data["audio_preview_url"], str(preview_path))
            logger.info("Downloaded audio preview to %s", preview_path)
        else:
            logger.warning("No audio preview URL found for track %s. Skipping audio download.", track_id)

        if track_data["cover_image_url"]:
            cover_path = output_dir / f"{track_id}_cover.jpg"
            download_file(track_data["cover_image_url"], str(cover_path))
            logger.info("Downloaded cover image to %s", cover_path)
        else:
            logger.warning("No cover image URL found for track %s. Skipping cover download.", track_id)
    except requests.exceptions.RequestException as e:
        logger.error("Error fetching track data: %s", e)
    except OSError as e:
        logger.error("File system error: %s", e)


if __name__ == "__main__":
    main()