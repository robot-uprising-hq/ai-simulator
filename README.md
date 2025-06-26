> [!WARNING]
> This repository has not been updated since late 2020 and may need old versions of dependencies or extensive modifications in order to work.

There's also a community built simulator, [Zero Ones Simulated](https://github.com/zero-ones-given/zero-ones-simulated), that can be used as a starting point for an environment for ML training, although integration with Unity ML Agents is not provided out of the box at least yet.

# AI Simulator

Read the summary of Micro Invaders from [here](https://github.com/robot-uprising-hq/ai-guide).

This is repository is based on Unity ML Agents Release 6 and the PushBlock example. This has been tested on Ubuntu 20.04, macOS Catalina 10.15.2, and Windows 10.


# Installation

## Prequisites

- python3 (3.6.1 or higher)
- Unity 2019.4.8f1 LTS
- Git

## Downloading the files

Download and install this repo and its Python dependencies using the installation scripts in [AI Guide repo](https://github.com/robot-uprising-hq/ai-guide)


# Using this repo

## Training robot
See document [here](docs/Training-the-Agent.md)

## Driving simulation with AI Remote Brain
See document [here](docs/Driving-with-AIRemoteBrain.md)

## Driving the simulation with AI Backend Connector
See document [here](docs/Driving-with-AIBackendConnector.md)

##  If you make changes to the gRPC protocol
See document [here](https://github.com/robot-uprising-hq/ai-guide/blob/master/docs/Generating-gRPC-code.md)

# Developing guide
## Changes to gRPC messages
In case you change a gRPC message check [this document](https://github.com/robot-uprising-hq/ai-guide/blob/master/docs/gRPC-code-generation.md)
