# Getting Started with this Workshop

In order for you to succeed in this workshop, we need you to run through a few steps to finalize the configuration of your Cloud9 environment. You could do this workshop in your own environment using an IDE like Visual Studio, but for a consistent experience for all users, we will walk through setting up a Cloud9 environment instead.

#### Update and install some tools

The first step is to update the `AWS CLI`, `pip` and a range of pre-installed packages.

```
sudo yum update -y && pip install --upgrade --user awscli pip
exec $SHELL
```

#### Configure the AWS Environment

After you have the installed the latest awscli and pip we need to configure our environment to use us-west-2

```
aws configure set region us-west-2

```

#### Install the .NET Core SDK

1. In this step, you install the .NET Core 3 SDK into your environment, which is required to run this sample. Run the below command to install a ***libunwind*** package that the .NET Core 3 SDK needs.

```
sudo yum -y install libunwind
```

2. Download the .NET Core 3 SDK installer script into your environment by running the following command.

```
curl -O -L https://dot.net/v1/dotnet-install.sh
```

3. Make the installer script executable by the current user by running the following command.

```
sudo chmod u=rx dotnet-install.sh
```

4. Run the installer script, which downloads and installs the .NET Core 3 SDK, by running the following command.

```
./dotnet-install.sh -c Current
```

5. Add the .NET Core 3 SDK to your PATH. To do this, in the shell profile for the environment (for example, the .bashrc file), add the $HOME/.dotnet subdirectory to the PATH variable for the environment, as follows.

  * Open the .bashrc file for editing by using the vi command.

```
vi ~/.bashrc
```

  * Using the down arrow or j key, move to the line that starts with ***export PATH***.

  * Using the right arrow or $ key, move to the end of that line.

  * Switch to insert mode by pressing the i key. (-- INSERT --- will appear at the end of the display.)

  * Add the $HOME/.dotnet subdirectory to the PATH variable by typing :$HOME/.dotnet. Be sure to include the colon character (:). The line should now look similar to the following.

![bashrc](/static/images/secrets/prerequisites/bashrc.png)

  * Save the file. To do this, press the Esc key (-- INSERT --- will disappear from the end of the display), type :wq (to write to and then quit the file), and then press Enter.

6. Load the .NET Core 3 SDK by sourcing the .bashrc file.

```
. ~/.bashrc
```

7. Confirm the .NET Core 3 SDK is loaded by running .NET Core CLI with the --help option.

```
dotnet --help
```

8. If successful, the .NET Core 3 SDK version number is displayed, with additional usage information.

#### Clone the source repository for this workshop

Now we want to clone the repository that contains all of the content and files you need to complete this workshop.

```
cd ~/environment

git clone https://github.com/aws-samples/modernization-unicorn-store.git
```

#### Installing Docker Compose

For this workshop we use the tool [Docker Compose](https://docs.docker.com/compose/) which is a tool for defining and running multi-container Docker applications.

1. Run this command to download the current stable release of Docker Compose:

```
sudo curl -L "https://github.com/docker/compose/releases/download/1.24.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
```

2. Apply executable permissions to the binary:

```
sudo chmod +x /usr/local/bin/docker-compose
```

3. Test the installation:
```
docker-compose --version
```

#### Install JQ

We will also be using a tool called [jq](https://stedolan.github.io/jq/) which is a lightweight and flexible command-line JSON processor.
```
sudo yum -y install jq
```

#### Clean up space on Cloud9 IDE

We are using one of the smaller Cloud9 instances and space is limited so we can clean up some space by removing these docker images we won't be using by running the following command:

```
docker rmi $(docker images -q)
```

#### Congratulations

You now have a fully working Cloud9 IDE that is ready to use! Click [**here**](/content/secrets/introduction.md) to start learning about secrets.
