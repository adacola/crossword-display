set PROJECT_NAME=crossword-display
set THIS_PATH=%~dp0
cd %THIS_PATH%\..
docker build -t %PROJECT_NAME% -f Dockerfile.build .
