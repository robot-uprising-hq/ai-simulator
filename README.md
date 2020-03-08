This has been tested on Ubuntu 18.04

Based on Unity ML Agents 0.14.0 PushBlock example
&nbsp;

&nbsp;

# Installation

## Clone this repo
```
git clone git@github.com:robot-uprising-hq/artificial-invaders-ai-unity-simulator.git
```

## Git clone the 0.14.0 Unity ML Agents
```
git clone --branch 0.14.0 https://github.com/Unity-Technologies/ml-agents.git
```

## Install ML-Agents Unity Package
Copy the `com.unity.ml-agents`-folder from the just cloned ml-agents-repo/folder to this project's `Package`-folder. You can now delete the `ml-agents`-repo's folder if you want.


## Create Python virtual environment if you want
```
virtualenv venv
source venv/bin/activate
```



---
## Install dependencies
```
pip install -r requirements.txt
```

Test mlagents-learn command
```
mlagents-learn --help
```
&nbsp;

&nbsp;

# Training the agent


## Train the agent
Start this project in Unity

Start mlagents-learn with the following command
```
mlagents-learn config/trainer_config.yaml --run-id=ai1.0 --train
```

Training with a Curriculum
```
mlagents-learn config/trainer_config.yaml --curriculum=config/curricula/pushblock.yaml --run-id=ai1.0 --train
```

Press the `Play` button in Unity and the training should start.

After the training is finished the new trained brain is created to the `models`-folder's subfolder which is called the same as the tag you used in the `--run`-argument in the training command.

&nbsp;

&nbsp;

# Testing Brain server and Robot Backend

## Download gRPC plugins
Unity ML-Agents already uses gRPC version 1.14.1 so let's take the closest build which is 1.15.0.

- Click the last Build ID with timestamp 2018-07-25T14:35:05-0700 on the Daily Builds of master Branch table [here](https://packages.grpc.io/)
- On the gRPC archive webpage download the right package for your system from the gRPC protoc Plugins table. For me on Ubuntu 18.04 it was grpc-protoc_linux_x64-1.15.0-dev.tar.gz
- Unpack the gRPC protoc Plugins file anywhere
- On the terminal set the path to the folder generated from the archive to an environmental variable `PROTOCPATH=/path/to/folder`



---
## Use simulation environment to test Unity brain server
Load scene `PushBlockBrainServerTest`. This scene sends observations to the Unity brain server which predicts an action based on the observations using the trained brain file.


### Generate gRPC code
If you have not made any changes to the .proto-file the correct generated files already exists in the repo and you can skip this part.

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

### Set up Unity Brain server
- Move the brain-file you just trained from `models`-folder to the Unity brain server and take the brain into use there.

- Start Unity Brain server

### Start simulation
- Start `PushBlockBrainServerTest` by pressing Unity's Play-button



---
## Use simulation environment to test robot backend
Load scene `PushBlockRobotBackendTest`. This scene sends screen capture of the simulation to the robot backend which detects from the image the robot and balls and further calculates the observations which it sends to Unity Brain server. The Unity brain server's action is then sent to Robot Backend which forwards it back to this simulation.


### Generate gRPC code
If you have not made any changes to the .proto-file the correct generated files already exists in the repo and you can skip this part.

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
    $INPUTPATH/RobotBackendCommunication.proto
```

### Set up Unity Brain server
- Move the brain-file you just trained from `models`-folder to the Unity brain server and take the brain into use there.

- Start Unity Brain server

### Start simulation
- Start `PushBlockBrainServerTest` by pressing Unity's Play-button
