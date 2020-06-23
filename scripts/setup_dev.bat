echo off

cd ..\PokeStar

echo "Copy env.json..."
copy .\env.json .\PokeStar\bin\Debug\env.json

echo "Createing image directories..."
cd .\PokeStar\bin\Debug
mkdir Images
cd .\Images
mkdir profile
mkdir ex_raid
mkdir raid

set /p exit = "Setup Complete. Press any key to continue..."