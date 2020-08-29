# Training the agent

1. Start AI Unity Simulator project in Unity and open the scene `AISimulation`.

1. Start mlagents-learn with the following commands in your project's root folder. The `<some-id>` is a string tag you give to identify this training run.
    ```sh
    source venv/bin/activate     # Activate virtual environment
    mlagents-learn config/trainer_config.yaml --run-id=<some-id>
    ```

1. After running the commands go to the Unity window and press the `Play` button in Unity to start the training process.

1. After the training is finished, the new trained brain is created to the `results`-folder's subfolder which is called the same as the `<some-id>`-tag you used in the `--run-id`-argument in the training command. The training process will run long, therefore, you might need to stop it by clicking the pause button in the Unity app.

1. Now, we have a trained model ready. The trained Brain file is called `PushBlock.nn`. in the `/results/<some-id>`-folder

## Training config file
The [config/trainer_config.yaml](config/trainer_config.yaml) file has two sections:
1. behaviors
1. environment_parameters

`behaviors`-section has the parameters to configure the neural network model. The `environment_parameters`-section is explained [here](https://github.com/Unity-Technologies/ml-agents/blob/release_6/docs/Training-ML-Agents.md#behavior-configurations)


The training uses curriculum learning and the config file's `environment_parameters`-section has the parameters which change the simulation as the agent gets better. The `environment_parameters`-section is explained [here](https://github.com/Unity-Technologies/ml-agents/blob/release_6/docs/Training-ML-Agents.md#curriculum)


For more information about the training process and parameters please follow the documentation of Unity at the following [link](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Training-ML-Agents.md).