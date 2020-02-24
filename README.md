This has been tested on Ubuntu 18.04

Based on Unity ML Agents 0.14.0 PushBlock example

## Clone this repo
```
git clone git@github.com:robot-uprising-hq/artificial-invaders-ai-unity-simulator.git
```

### Git clone the 0.14.0 Unity ML Agents
```
git clone --branch 0.14.0 https://github.com/Unity-Technologies/ml-agents.git
```

### Install ML-Agents Unity Package
Copy the `com.unity.ml-agents`-folder from the just cloned ml-agents-repo/folder to this project's `Package`-folder. You can now delete the `ml-agents`-repo's folder if you want.


### Create Python virtual environment if you want
```
virtualenv venv
source venv/bin/activate
```

---
### Install dependencies
```
pip install -r requirements.txt
```

Test mlagents-learn command
```
mlagents-learn --help
```

---
### Train the agent
Start this project in Unity

Start mlagents-learn with the following command
```
mlagents-learn config/trainer_config.yaml --run-id=ai1.0 --train
```

Press the `Play` button in Unity and the training should start.