This has been tested on Ubuntu 18.04

Based on Unity ML Agents 0.14.0 PushBlock example

## Clone this repo
git clone git@github.com:robot-uprising-hq/artificial-invaders-ai-unity-simulator.git


## Unity ML Agents installation



### Git clone the 0.14.0 Unity ML Agents
```
git clone --branch 0.14.0 https://github.com/Unity-Technologies/ml-agents.git
```

### Install ML-Agents Unity Package
Copy the com.unity.ml-agents-folder from the just cloned ml-agents-repo/folder to this project's Package-folder.


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

Test ml-agents
```
mlagents-learn --help
```

---
### Train the agent
mlagents-learn config/trainer_config.yaml --run-id=ai1.0 --train
