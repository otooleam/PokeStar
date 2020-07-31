echo off

cd ..\PokeStar

echo "Copying env file..."
copy .\env.json .\PokeStar\bin\Debug\env.json

echo "Copy Pokemon Images..."
xcopy .\Images\PokemonImages .\PokeStar\bin\Debug\PokemonImages

echo "Createing required directories..."
cd .\PokeStar\bin\Debug
mkdir Images
mkdir Logs
cd .\Images
mkdir profile
mkdir ex_raid
mkdir raid

set /p exit = "Setup Complete. Press any key to continue..."