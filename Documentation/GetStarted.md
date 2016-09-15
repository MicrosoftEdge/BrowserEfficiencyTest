# Get started

This guide will walk through getting started with BrowserEfficiencyTest.

## Assumptions

* You're on a Windows device
* You have the code available in the [BrowserEfficiencyTest repo](https://github.com/MicrosoftEdge/BrowserEfficiencyTest) cloned or downloaded on your device
* You have Visual Studio or something similar. Try [Visual Studio Community](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx), it's free

## Get the dependencies

* First, go download or clone [Elevator](https://github.com/MicrosoftEdge/Elevator). Elevator is used to trace while a browser is automatically running through scenarios. This results in a trace file (with a .etl extension), which will later be used to extract measures, like how much CPU, networking, or power was used while the browser was working. Traces can also be opened in Windows Performance Analyzer, so you can see what was going on during the test.
* Next, download the Windows Performance Toolkit. It's available as part of the Windows Assessment and Deployment Kit (ADK), and can be downloaded [here](http://go.microsoft.com/fwlink/p/?LinkId=526740).
    * In the ADK installer, select "Install to this computer"
	* Keep the default path
	* In "Select the features you want to install", Uncheck every item except "Windows Performance Toolkit".

## Webdriver

Most browsers require you to have the correct version of their associated webdrive exe in PATH. You'll need to follow these instructions for each browser you wish to test on.

### Step 1. Make a folder for webdriver

First, make a folder that you'll put your webdriver exes in, anywhere on your device. This will be added to your PATH variable, allowing BrowserEfficiencyTest to control browsers on your device.

### Step 2a. Download Microsoft Edge Webdriver

Download the exe from [here](https://developer.microsoft.com/en-us/microsoft-edge/tools/webdriver/). Not sure which release is the correct one? Type `winver` into Cortana, and look for the nubmerbeside "OS Build".

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

## Run the test