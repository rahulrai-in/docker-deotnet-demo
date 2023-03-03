using Docker.DotNet;
using Docker.DotNet.Models;

var dockerClient = new DockerClientConfiguration().CreateClient();

async Task CreateNetwork()
{
    var networkCreateParameters = new NetworksCreateParameters { Name = "my-network" };
    var response = await dockerClient.Networks.CreateNetworkAsync(networkCreateParameters);
    Console.WriteLine($"Network with ID {response.ID} created");
}

async Task ListNetworks()
{
    var networks = await dockerClient.Networks.ListNetworksAsync();
    foreach (var network in networks)
    {
        Console.WriteLine($"Network ID: {network.ID}, Name: {network.Name}");
    }
}

async Task ConnectContainerToNetwork()
{
    // Pull image
    await dockerClient.Images.CreateImageAsync(new()
    {
        FromImage = "nginx",
        Tag = "1.23.3"
    }, new(), new Progress<JSONMessage>());
    // Create container
    var nginxContainer = await dockerClient.Containers.CreateContainerAsync(new()
    {
        Image = "nginx:1.23.3",
        Name = "nginx",
    });
    await dockerClient.Containers.StartContainerAsync(nginxContainer.ID, new());
    await dockerClient.Networks.ConnectNetworkAsync("my-network", new()
    {
        Container = nginxContainer.ID
    });
    Console.WriteLine("Container attached to network");
}

async Task DisconnectContainerFromNetwork()
{
    var container = await dockerClient.Containers.ListContainersAsync(new()
    {
        Filters = new Dictionary<string, IDictionary<string, bool>>
        {
            { "name", new Dictionary<string, bool> { { "nginx", true } } },
        },
    });
    await dockerClient.Networks.DisconnectNetworkAsync("my-network", new()
    {
        Container = container[0].ID
    });
    Console.WriteLine("Container disconnected from network");
}

async Task DeleteNetwork()
{
    await dockerClient.Networks.DeleteNetworkAsync("my-network");
    Console.WriteLine("Network deleted");
}

while (true)
{
    Console.WriteLine("\nChoose an option:");
    Console.WriteLine("1. Create a Docker network");
    Console.WriteLine("2. List Docker networks");
    Console.WriteLine("3. Connect a container to a Docker network");
    Console.WriteLine("4. Disconnect a container from a Docker network");
    Console.WriteLine("5. Delete a Docker network");
    Console.WriteLine("6. Exit");

    var option = Console.ReadLine();

    switch (option)
    {
        case "1":
            await CreateNetwork();
            break;
        case "2":
            await ListNetworks();
            break;
        case "3":
            await ConnectContainerToNetwork();
            break;
        case "4":
            await DisconnectContainerFromNetwork();
            break;
        case "5":
            await DeleteNetwork();
            break;
        case "6":
            return;
        default:
            Console.WriteLine("Invalid option");
            break;
    }
}