public enum NetMessageType
{
    Move
}

public interface INetClient
{
    void Shoot();
    void Move(PlayerController.SimulationState simulation, PlayerController.InputStroke[] input, float timestep);
}

public interface INetServer
{
    void Shoot();
    void Move(int id, PlayerController.SimulationState simulation, PlayerController.InputState input, float timestep);
}