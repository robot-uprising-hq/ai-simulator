# Micro AI Simulation

Read the summary of Micro Invaders from [here](https://github.com/robot-uprising-hq/artificial-invaders-guide).

This is repository is based on Unity ML Agents 0.14.0 PushBlock example. This has been tested on Ubuntu 18.04, macOS Catalina 10.15.2, and Windows 10.

# Installation

### Prequisites

- Python 3.7.x
- Unity 2019.3.x
- Git

### Downloading the files

Clone the repo
```
git clone git@github.com:robot-uprising-hq/artificial-invaders-ai-unity-simulator.git
```
Checkout to the branch
```
git checkout dev/sending-screencapture-to-external-server
```
Open the given project, i.e. AI Unity Simulator, in Unity. After opening the project, you can close Unity for a moment. 

Opening the project has created a folder “Packages” in the project's main folder. You should add a library “com.unity.ml-agents” to the “Packages” to make it possible to utilize the ml-agents. Ml-agents is Unity's Machine Learning Agents Toolkit. It is an open-source project that enables Unity projects to serve as a training environment for intelligent agents.

The “com.unity.ml-agents” library can be found from Github:

```
git clone --branch 0.14.0 https://github.com/Unity-Technologies/ml-agents.git
```

Inside the repository, is `com.unity.ml-agents`-folder. 

Copy `com.unity.ml-agents`-folder to the project's `Packages`-folder. 


### Creating virtual environment for Python (optional)

Unity ml-agents is running over Python 3. Creating virtual environment for the Python is recommend before installing requirements. Note: Python version should be 3.7.x.

The following command creates isolated Python environment containing their own copy of python, pip, and their own place to keep libraries installed from PyPI. It's designed to allow you to work on multiple projects with different dependencies at the same time on the same machine.


If you have not installed virtualenv tool, you need to run (macOS, Ubuntu) ``` pip install virtualenv ``` or ``` pip3 install virtualenv ``` and then run: ```sudo /usr/bin/easy_install virtualenv ```

Create a new virtual env (macOS, Ubuntu):
```
virtualenv venv
source venv/bin/activate
```

Create a new virtual env (windows):
```
virtualenv env
\env\Scripts\activate.bat
```

### Installing Python dependencies

Change directory to the folder of the repository. (replace the “folder_path” accordingly) 

```
cd folder_path/artificial-invaders-ai-unity-simulator
```

Install requirements
```
pip install -r requirements.txt
```

Verify that everything is installed correctly
```
mlagents-learn --help
```

If you got error ```RuntimeError: dictionary changed size during iteration```, the issue usually hints that TensorFlow is crashing, which can happen if Python version is 3.7.3.  You can resolve this issue by upgrading the python version to 3.7.5 or 3.7.7:

Tip your python version with the command ```which python``` (macOs, Ubuntu).

---
# Training the agent 

### Train without a curriculum
Start AI Unity Simulator project in Unity and open the scene “PushBlockSimulation”.

Start mlagents-learn with typing the following command in your project's folder
```
mlagents-learn config/trainer_config.yaml --run-id=ai1.0 --train
```

After running command go to the Unity App and press the `Play` button in Unity to start the training process.

N.B: If you are having errors in the console when you press the “Play” button. Try checking out that you are in right git branch. Also, try adding the project again in Unity Hub to make sure that the changes have been reflected. 

After the training is finished, the new trained brain is created to the `models`-folder's subfolder which is called the same as the tag you used in the `--run-id`-argument in the training command. In our example, it will be “ai1.0”

The training process will run forever, therefore, you might need to stop it by clicking the pause button in the Unity app.

Now, we have a trained model "ai1.0” ready.

### Train with a curriculum

Same as above, but use the command

```
mlagents-learn config/trainer_config.yaml --curriculum=config/curricula/pushblock.yaml --run-id=ai1.0
```

For more information about the training process and parameters please follow the documentation of Unity at the following link. https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Training-ML-Agents.md


---

# Testing Brain Server and Robot Backend

In this point, your simulation should be running, and you should able to train your agents. Next steps are going to show, how to use trained agents (Brain Server), and how to transfer the model in real world scenarios (Robot Backend).

## Prerequisite

The different components communicates through gRPC protocol. For instance Brain Server communicates to Robot Backend and also to Simulaton through gRPC:

Download gRPC plugins. gRPC is a Remote Procedure Call developed by google, it is an open source high performance RPC framework that can run in any environment. More information at https://grpc.io/.


Unity ML-Agents already uses gRPC version “1.14.1” so let's take the closest build which is “1.15.0”.

- Click the last Build ID with timestamp 2018-07-25T14:35:05-0700 on the Daily Builds of master Branch table [here](https://packages.grpc.io/)
- On the gRPC archive webpage download the right package for your system from the gRPC protoc Plugins table. For me on Ubuntu 18.04 it was grpc-protoc_linux_x64-1.15.0-dev.tar.gz
- Unpack the gRPC protoc Plugins file anywhere
- On the terminal set the path to the folder generated from the archive to an environmental variable `PROTOCPATH=/path/to/folder`


## Use simulation to test Brain Server

In the “artificial-invaders-ai-unity-simulator” project, load the scene `PushBlockBrainServerTest`. This scene sends observations to the Unity brain server which predicts an action based on the observations using the trained brain file.


### Generate gRPC code (optional)

gRPC code is based .proto-files. The required C# code for the Unity is generated by using those .proto files. If you have not made any changes to the .proto-file, you can skip this step. The pregenerated files already exist in the repository.

Set environmental variables for the input and output path for the protoc-executable to generate the gRPC code from the .proto-file
- INPUTPATH=/path/to/Assets/Proto
- OUTPUTPATH=/path/to/Assets/Proto/GeneratedCode

Run the following command to generate the gRPC code from the .proto-file:
```
$PROTOCPATH/protoc \
    --plugin=protoc-gen-grpc=$PROTOCPATH/grpc_csharp_plugin \
    --proto_path=$INPUTPATH \
    --csharp_out=$OUTPUTPATH \
    --grpc_out=$OUTPUTPATH \
    $INPUTPATH/BrainCommunication.proto
```


You may get the error message: “zsh: permission denied: /folder_path/grpc-protoc_macos_x64-1.15.0-dev/protoc”. This hints “protoc” does not have executable permission, which you could confirm by:
```
ls -l $PROTOCPATH/protoc 
```

**TODO: May need some refactoring, at least I don't understand Windows part?**

If it gives a message starting with “-rw-r--r-”, it means the “protoc” file is not executable. To resolve this issue, set “roto_path”, “csharp_out” and “grpc_out” in a similar way as above for Windows 10.

Different from the way for Windows, in macOS, you need to install the following python modules to run the generator:
```
python -m pip install grpcio
python -m pip install grpcio-tools
```
and then run
```
python -m grpc_tools.protoc
```

### Set up Brain Server

Clone Brain Server repo
```
git clone https://github.com/robot-uprising-hq/artificial-invaders-ai-unity-brain-server.git
```

After opening Brain Server in Unity, copy the “com.unity.ml-agents” to the “Packages” folder “artificial-invaders-ai-unity-brain-server” as with AI Unity Simulation.

Next, you have to move your trained model from AI Unity Simulation to Brain server.

Copy the brain-folder, i.e. “[pathToAIUnitySimulation]/models/ai1.0” you have trained. 

Create a folder called “TFModels” inside "Assets" folder of the Brain Server, and paste the folder “ai1.0” inside it.

Then take the brain file into use by clicking the hierarchy view in Brain Server's Unity project, and then choose PushBlockRemoteUsage -> Area -> RemoteAgent. From the inspector (that should be on right side of Unity defaul's view), extend the Behaviour Parameters and choose the NN file from the brain-folder, i.e. "ai1.0", as the Model.


### Run the simulation with Robot Backend

- In the AI Unity Simulator project, start the scene `PushBlockBrainServerTest` by pressing Unity's Play-button
- In the Brain Server project, Start the server connection by pressing Unity's Play-button.

## Use simulation environment to test Robot Backend

In the AI Unity Simulation project, load scene `PushBlockRobotBackendTest`. This scene sends a screen capture of the simulation to the robot backend which detects from the image the robot and balls and further calculates the observations which it sends to Unity Brain server. The Unity brain server's action is then sent to Robot Backend which forwards it back to this simulation.


### Generate gRPC code (optional)
If you have not made any changes to the .proto-file, you can skip this part.

Set environmental variables for the input and output path for the protoc-executable to generate the gRPC code from the .proto-file
- INPUTPATH=/path/to/Assets/Proto
- OUTPUTPATH=/path/to/Assets/Proto/GeneratedCode
Run the following command to generate the gRPC code from the .proto-file:
```
$PROTOCPATH/protoc \
    --plugin=protoc-gen-grpc=$PROTOCPATH/grpc_csharp_plugin \
```



