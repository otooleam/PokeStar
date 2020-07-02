echo off

echo "Createing production directory..."
mkdir PokeStar_Production

echo "Copy env.json..."
copy ..\PokeStar\env.json .\PokeStar_Production\env.json

echo "Copy Pokemon Images..."
xcopy ..\PokeStar\Images\PokemonImages .\PokeStar_Production\PokemonImages

echo "Copy PokeStar.exe..."
copy ..\PokeStar\PokeStar\bin\Debug\PokeStar.exe .\PokeStar_Production\PokeStar.exe

echo "Createing image directories..."
cd PokeStar_Production

mkdir Images
cd ./Images
mkdir profile
mkdir ex_raid
mkdir raid

echo
echo "Executable and supporting files now in PokeStar_Production"
set /p exit = "Setup Complete. Press any key to continue..."