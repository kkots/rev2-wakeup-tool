#!/bin/bash
# Make sure to give yourself permission to launch this script with 'chmod u+x launch_GGXrdReversalTool_linux.sh' command.
# Then cd into the same directory as this script and launch it using './launch_GGXrdReversalTool_linux.sh' or 'bash launch_GGXrdReversalTool_linux.sh'.

# This script launches the mod's .exe in Wine under the same wineserver as GuiltyGearXrd.exe process so that the mod can actually find the Guilty Gear process.

# This mod depends on .NET Desktop Runtime version  6.0.
# You can use the install_dotnet.sh script to install it. It must be installed in the same Wine environment
# that Guilty Gear Xrd runs in and that script takes care of that.
# Once the .NET runtime is installed, it will stay installed under that WINEPREFIX (virtual environment) forever, there's no need to install it again.

# To install .NET Runtime manually you can:
# Download the 64-bit Windows (not Linux!) installer from Microsoft's website:
# https://dotnet.microsoft.com/en-us/download/dotnet/6.0
# The (Windows) installer will have to be launched (on Linux) through Wine.
# Navigate to the foler where you downloaded the .NET installer and launch it under Wine under the Guilty Gear's WINEPREFIX with WINEFSYNC set to 1:
# ```bash
# cd ~   # replace with the actual folder where the .NET installer is
# WINEFSYNC=1 WINEPREFIX=$MYWINEPREFIX "$PATHTOWINE" dotnet-sdk-6.0.405-win-x64.exe
# # the .NET version here is an example, I don't know the exact version
# ```
# If you don't know the WINEPREFIX (virtual environment) or PATHTOWINE variables, you could use this script to launch the .NET installer.
# To do so, simply change the TOOLNAME variable below to the name of the .NET executable, or launch this script with
# an extra argument where you specify the dotnet installer name, like so:
# ./launch_GGXrdReversalTool_linux.sh dotnet-sdk-6.0.405-win-x64.exe
# This .sh script must be placed into the same folder as the exe before being run or you can provide the full path in the argument or TOOLNAME.
# Once the .NET runtime is installed, it will stay installed under that WINEPREFIX (virtual environment) forever, there's no need to install it again.


# This mod's tool that we want to launch.
TOOLNAME="${1-GGXrdReversalTool.exe}"  # ${VARNAME-DEFAULTVALUE} substitutes DEFAULTVALUE in case the variable is empty
PRINT_DOTNET_WARNING=$([[ -z $1 ]] && echo true || echo false)
ONLY_PRINT_WINEPREFIX=false
if [ "$2" == "--only-print-wineprefix" ]; then
  ONLY_PRINT_WINEPREFIX=true
fi

if [ ONLY_PRINT_WINEPREFIX == false ] && [ ! -f "$TOOLNAME" ]; then
    echo "$TOOLNAME" not found in the current directory. Please cd into this script\'s directory and launch it again.
    exit 1
fi

GUILTYGEAR_PID=$(pidof -s GuiltyGearXrd.exe)
if [ $? != 0 ]; then # if last command's (pidof's) exit code is not success
    echo Guilty Gear Xrd not launched. Please launch it and try again.
    exit 1
fi

GUILTYGEAR_WINEPREFIX=$(
    
    # The environment variables the process was launched with are stored in
    # /proc/123456789/environ file.
    # Each entry is in the format VARIABLE_NAME=VALUE and the entries are
    # separated by the null ('\0') character.
    cat /proc/$GUILTYGEAR_PID/environ                                                              \
                                                                                                   \
     `# Replaces the null character with a newline so that grep can work with it.`                 \
    | tr '\0' '\n'                                                                                 \
                                                                                                   \
     `# Finds lines starting with WINEPREFIX=.`                                                    \
     `# We need the WINEPREFIX variable so we can launch our mod with the same`                    \
     `# WINEPREFIX as Guilty Gear so that it can see Gealty Gear's process or find Steam files.`      \
     `# WINEPREFIX basically uniquely identifies a wineserver. A bunch of Wine programs`           \
     `# with the same WINEPREFIX run on the same wineserver and if the WINEPREFIXes are different,`\
     `# then they run on different wineservers and are completely isolated.`                       \
    | grep "^WINEPREFIX="                                                                          \
                                                                                                   \
     `# Stops reading the file through cat if there's already one line of output.`                 \
    | head -n1                                                                                     \
    | sed -r 's/^WINEPREFIX=//'  # Replaces the initial WINEPREFIX= with nothing, so we only get
                                 # the value part.
)
if [ "$GUILTYGEAR_WINEPREFIX" == "" ]; then
    echo "Couldn't determine Guilty Gear Xrd's WINEPREFIX."
    exit 1
fi

if [ $ONLY_PRINT_WINEPREFIX == true ]; then
  echo "$GUILTYGEAR_WINEPREFIX"
  exit
fi

GUILTYGEAR_WINELOADER=$(
    cat /proc/$GUILTYGEAR_PID/environ                                                      \
    | tr '\0' '\n'                                                                         \
                                                                                           \
     `# Finds lines starting with WINELOADER=.`                                            \
     `# The WINELOADER variable specifies the location of the Wine executable that`        \
     `# this Wine program was launched with. So even if you have multiple wineservers or`  \
     `# multiple Wine versions installed and running concurrently, we'll determine and use`\
     `# the exact same version as Guilty Gear is using.`                                   \
    | grep "^WINELOADER="                                                                  \
    | head -n1                                                                             \
    | sed -r 's/^WINELOADER=//'
)
if [ "$GUILTYGEAR_WINELOADER" == "" ]; then
    echo "Couldn't determine Guilty Gear Xrd's WINELOADER."
    exit 1
fi

if [ $PRINT_DOTNET_WARNING == true ]; then
  echo If the program gives error message \"You must install .NET Desktop Runtime to run this application.\" you can run the ./install_dotnet.sh to install it in the same environment Guilty Gear Xrd runs in.
fi

# The 2> /dev/null pipes stderr (error output) into nowhere.
# We need this because if this is a console app it will run in your current terminal window,
# and Wine often says irrelevant stuff into stderr during any program and even breaks in
# the middle of what the program is saying, and your terminal would display that as well if
# we don't silence it.
# The WINEFSUNC variable is needed because Wine would refuse to launch otherwise.
WINEFSYNC=1 WINEPREFIX="$GUILTYGEAR_WINEPREFIX" "$GUILTYGEAR_WINELOADER" $TOOLNAME 2> /dev/null
