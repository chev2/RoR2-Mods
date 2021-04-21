import os
import zipfile

# zipfiles.py
# Takes all folders in the .thunderstore/ folder and puts them into .zip files
# Essentially prepares mods for uploading to Thunderstore

def zipfiles(z_obj, dir):
    for root, dirs, files in os.walk(dir):
        for file in files:
            file_loc = file if not file.endswith('.dll') else f'plugins/Chev/{file}'
            z_obj.write(os.path.join(root, file), arcname=file_loc)

for folder in next(os.walk('.'))[1]:
    print(f'{folder}/')
    with zipfile.ZipFile(f'{folder}.zip', 'w') as z:
        zipfiles(z, f'{folder}/')
