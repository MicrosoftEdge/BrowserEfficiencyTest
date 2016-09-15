# Get started

This guide will walk through getting started with BrowserEfficiencyTest.

## Assumptions

* You're on a Windows device
* You have the code available in the [BrowserEfficiencyTest repo](https://github.com/MicrosoftEdge/BrowserEfficiencyTest) cloned or downloaded on your device
* You have Visual Studio or equivalent. Try [Visual Studio Community](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx), it's free

## Get the dependencies

* First, go download or clone [Elevator](https://github.com/MicrosoftEdge/Elevator). Elevator is used to trace while a browser is automatically running through scenarios. This results in a trace file (with a .etl extension), which will later be used to extract measures (e.g. how much CPU, networking, or power was used while the browser was working). Traces can also be opened in Windows Performance Analyzer, so you can see what was going on during the test.
* Next, download the Windows Performance Toolkit. It's available as part of the Windows Assessment and Deployment Kit (ADK), and can be downloaded [here](http://go.microsoft.com/fwlink/p/?LinkId=526740).
    * In the ADK installer, select "Install to this computer"
	* Keep the default path
	* In "Select the features you want to install", Uncheck every item except "Windows Performance Toolkit".

## Webdriver

This is how you set Webdriver up. You must have the correct version of the associated Webdriver exe in PATH for every browser you wish to test.

### Step 1. Make a folder for Webdriver

First, make a folder that you'll put your webdriver exes in, anywhere on your device. This will be added to your PATH variable, allowing BrowserEfficiencyTest to control browsers on your device.

### Step 2a. Download Microsoft Edge Webdriver

Download the exe from [here](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/). Not sure which release is the correct one? Type `winver` into Cortana, and look for the nubmer beside "OS Build".

Put `MicrosoftWebDriver.exe` in the folder from step 1.

### Step 2b. Download Chrome Webdriver

Download the exe from [here](https://sites.google.com/a/chromium.org/chromedriver/downloads).

Unzip it and put `chromedriver.exe` in the folder from step 1.

### Step 2c. Download Firefox Webdriver

Firefox is transitioning from GeckoDriver to MarionetteDriver. At the time of this writing, MarionetteDriver was not stable with BrowserEfficiencyTest, and so testing has been done on version 47.0.1 of Firefox using GeckoDriver. This is likely to change as MarionetteDriver's stability improves.

GeckoDriver is avilable [here](https://github.com/mozilla/geckodriver/releases).

Extract `geckodriver.exe` to the folder from step 1.

### Step 2d. Download Opera Webdriver

Use the OperaChromiumWebdriver available [here](https://github.com/operasoftware/operachromiumdriver/releases).

Download the Win32 version and extract `operadriver.exe` to the folder from step 1.

### Step 3. Add the folder to PATH

You should now have a folder on your device with the respective Webdriver for each browser you wish to test.

* Go to "System" (which you can search for through Cortana)
* Click on "Advanced System Settings"
* Click on "Environment Variables"
* Under "System Variables", select "Path" and then click on "Edit..."
* Click on "New" and paste in the location of the folder from step 1
* Click OK

## Build the solutions

* In the directory you cloned/downloaded Elevator to, open the solution file in Visual Studio and build it.
* In the directory you cloned/downloaded BrowserEfficiencyTest to, open the solution file in Visual Studio and build it.

## Configuration

* In Microsoft Edge, go to settings, "View advanced settings", then turn "Block pop-ups" to off. This is required in order for BrowserEfficiencyTest to open new tabs in Microsoft Edge.

### Recommendations to reduce variability

The items in this section are not required, but they are useful recommendations for making your tests more repeatable and consistent. These will reduce interference from other factors than the browsers and sites you wish to test.

* Turn off adaptive brightness of the screen if applicable
* Turn off bluetooth
* Turn off location
* Ensure your volume is set to the same level as other tests you have/will run if applicable
* Battery saver mode will activate when the battery of your device reaches 20%. Disable this if applicable
* Ensure bitlocker is disabled or consistent with other tests you have/will run

### If you're measuring power

* From an elevated command prompt, run:
    `reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SRUM\Parameters" /v Tier1Period /t REG_DWORD /d 10 /f`
	This will set the energy estimation engine in Windows to fire an event every 10s.
* Ensure that the drivers are installed if you have power-measuring hardware like a [Maxim 34407 Power Accumulator chip](https://www.maximintegrated.com/en/products/analog/amplifiers/MAX34407.html)

## Run the test

### Start Elevator

* Run Elevator by going to the its folder, then within it, navigating to \ElevatorServer\bin\Debug\ElevatorServer.exe (assuming you built for Debug).
* Accept the UAC prompt
* The window should say "Waiting for client connection."

### Start the test

* Run BrowserEfficiencyTest by going to its folder, then within it, navigating to \BrowserEfficiencyTest\bin\Debug\ (assuming you built for Debug).
* Open a command window here if you're not already in the command line (by shift + right-clicking in the folder and selecting "Open command window here")