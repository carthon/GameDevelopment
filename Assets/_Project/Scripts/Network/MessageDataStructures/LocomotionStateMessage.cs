using _Project.Scripts.Components;
using _Project.Scripts.Network.MessageUtils;
using RiptideNetworking;
using UnityEngine;

namespace _Project.Scripts.Network.MessageDataStructures {
    public enum LocomotionMode : int { Grounded, Airborne, Fly }
    public struct LocomotionStateMessage : IGenericMessageStruct {
        public ushort id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 localDirection;
        public Vector3 forwardDirection;
        public Quaternion modelRotation;
        public Quaternion headRotation;
        public PlanetData planetData;
        public int tick;
        public ulong actions;
        public LocomotionMode mode;
        public bool isGrounded;
        
        public LocomotionStateMessage(ushort id, Vector3 position, Vector3 velocity, Vector3 localDirection, Vector3 forwardDirection, Quaternion modelRotation, Quaternion headRotation, PlanetData planetData, int tick, ulong actions, LocomotionMode mode, bool isGrounded) {
            this.id = id;
            this.position = position;
            this.velocity = velocity;
            this.localDirection = localDirection;
            this.forwardDirection = forwardDirection;
            this.modelRotation = modelRotation;
            this.headRotation = headRotation;
            this.planetData = planetData;
            this.tick = tick;
            this.actions = actions;
            this.mode = mode;
            this.isGrounded = isGrounded;
        }
        public LocomotionStateMessage(Message message) {
            id = message.GetUShort();
            position = message.GetVector3();
            velocity = message.GetVector3();
            localDirection = message.GetVector3();
            forwardDirection = message.GetVector3();
            modelRotation = message.GetQuaternion();
            headRotation = message.GetQuaternion();
            tick = message.GetInt();
            actions = message.GetULong();
            mode = (LocomotionMode) message.GetInt();
            isGrounded = message.GetBool();
            planetData = message.GetPlanetData();
        }
        public void Serialize(Message message) {
            message.AddUShort(id).AddVector3(position).AddVector3(velocity).AddVector3(localDirection).AddVector3(forwardDirection).AddQuaternion(modelRotation).AddQuaternion(headRotation)
                .AddInt(tick).AddULong(actions).AddInt((int)mode).AddBool(isGrounded).AddPlanetData(planetData);
        }
        
        public override string ToString() {
            return $"ClientId:{id}, " +
                $"Position:{position.ToString()}, " +
                $"LocalDirection:{localDirection.ToString()}, " +
                $"ForwardDirection:{forwardDirection.ToString()}, " +
                $"Rotation: {modelRotation}" +
                $"HeadRotation: {headRotation}" +
                $"Velocity: {velocity}" +
                $"Grounded: {isGrounded}" +
                $"Tick:{tick}";
        }
    }
}