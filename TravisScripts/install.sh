#! /bin/sh

# Downloads
echo 'Downloading Unity-2017.2.0f3 pkg:'
curl --retry 5 -o Unity.pkg https://netstorage.unity3d.com/unity/46dda1414e51/MacEditorInstaller/Unity-2017.2.0f3.pkg
#curl --retry 5 -o Unity.pkg https://netstorage.unity3d.com/unity/a9f86dcd79df/MacEditorInstaller/Unity-2017.3.0f3.pkg
if [ $? -ne 0 ]; then { echo "Unity Download failed"; exit $?; } fi

# Install
echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /

echo "Verify firewall"
/usr/libexec/ApplicationFirewall/socketfilterfw --getappblocked /Applications/Unity/Unity.app/Contents/MacOS/Unity

echo "Create Certificate Folder"
mkdir ~/Library/Unity
mkdir ~/Library/Unity/Certificates

cp "./TravisScripts/CACerts.pem" ~/Library/Unity/Certificates/

echo === Done ===
