import requests
import re
import json
import html

track_id = "6SkGfPa77E4giShVbk9N6R"
embed_url = f"https://open.spotify.com/embed/track/{track_id}"
try:
    response = requests.get(embed_url)
    response.raise_for_status()
    html_content = response.text
    json_next_data_match = re.search(
        r'<script\s+id="__NEXT_DATA__"\s+type="application/json">(.*?)</script>',
        html_content,
        re.DOTALL,
    )
    if json_next_data_match:
        json_raw = html.unescape(json_next_data_match.group(1)).strip()
        json_next_data = json.loads(json_raw)
        artists = json_next_data['props']['pageProps']['state']['data']['entity']['artists']
        artist_names = [artist['name'] for artist in artists]
        print("Artists:", ", ".join(artist_names))
        title = json_next_data['props']['pageProps']['state']['data']['entity']['name']
        print("Title:", title)
        cover_image_url = json_next_data['props']['pageProps']['state']['data']['entity']['visualIdentity']['image'][2]['url']
        print("Cover Image URL:", cover_image_url)
        audio_preview_url = json_next_data['props']['pageProps']['state']['data']['entity']['audioPreview']['url']
        print("Audio Preview URL:", audio_preview_url)
        # Download the audio preview
        audio_response = requests.get(audio_preview_url)
        audio_response.raise_for_status()
        with open(f"{track_id}_preview.mp3", "wb") as audio_file:
            audio_file.write(audio_response.content)
        # Download the cover image
        image_response = requests.get(cover_image_url)
        image_response.raise_for_status()
        with open(f"{track_id}_cover.jpg", "wb") as image_file:
            image_file.write(image_response.content)
    else:
        print("No __NEXT_DATA__ script tag found in the page.")
except requests.exceptions.RequestException as e:
    print("Error fetching track data:", e)