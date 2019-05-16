using LiteNetLib.Utils;

using System;
using System.Collections.Generic;

using UnityEngine;
using Wheeled.Gameplay.Action;
using Wheeled.Gameplay.Movement;
using Wheeled.Gameplay.Player;

namespace Wheeled.Networking
{
    public static class Serializer
    {
        public static readonly NetDataWriter writer = new NetDataWriter(true, 128);

        public static void WriteDiscoveryInfo(int _arena)
        {
            writer.Reset();
            writer.PutAsByte(_arena);
        }

        public static void WriteMovementAndInputReplication(int _id, int _step, IEnumerable<InputStep> _inputSteps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(EMessage.MovementReplication);
            writer.PutAsByte(_id);
            writer.Put(_step);
            writer.Put(_snapshot);
            writer.Put(_inputSteps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteMovementNotify(int _firstStep, IEnumerable<InputStep> _steps, in Snapshot _snapshot)
        {
            writer.Reset();
            writer.Put(EMessage.MovementNotify);
            writer.Put(_firstStep);
            writer.Put(_snapshot);
            writer.Put(_steps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteSimulationCorrection(int _step, in SimulationStepInfo _simulationStepInfo)
        {
            writer.Reset();
            writer.Put(EMessage.SimulationOrder);
            writer.Put(_step);
            writer.Put(_simulationStepInfo);
        }

        public static void WriteDamageOrderOrReplication(double _time, int _id, DamageInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.DamageOrderOrReplication);
            writer.Put(_time);
            writer.PutAsByte(_id);
            writer.Put(_info);
        }

        public static void WriteKazeNotify(double _time, KazeInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.KazeNotify);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteShootNotify(double _time, ShotInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.ShootNotify);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WriteShootReplication(double _time, int _id, ShotInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.ShootReplication);
            writer.Put(_time);
            writer.PutAsByte(_id);
            writer.Put(_info);
        }

        public static void WriteSpawnOrderOrReplication(double _time, int _id, SpawnInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.SpawnOrderOrReplication);
            writer.Put(_time);
            writer.PutAsByte(_id);
            writer.Put(_info);
        }

        public static void WritePlayerIntroductionSync(int _id, PlayerInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.PlayerIntroductionSync);
            writer.PutAsByte(_id);
            writer.Put(_info);
        }

        public static void WritePlayerWelcomeSync(int _id, int _arena)
        {
            writer.Reset();
            writer.Put(EMessage.PlayerWelcomeSync);
            writer.PutAsByte(_id);
            writer.PutAsByte(_arena);
            // Checksum
            writer.PutAsByte(255 - _id);
            writer.PutAsByte(_id);
            writer.PutAsByte(255 - _id);
        }

        public static void WriteQuitReplication(double _time, int _id)
        {
            writer.Reset();
            writer.Put(EMessage.QuitReplication);
            writer.Put(_time);
            writer.PutAsByte(_id);
        }

        public static void WriteReady()
        {
            writer.Reset();
            writer.Put(EMessage.ReadyNotify);
        }

        public static void WriteRecapSync(double _time, IEnumerable<PlayerRecapInfo> _recaps)
        {
            writer.Reset();
            writer.Put(EMessage.RecapSync);
            writer.Put(_time);
            writer.Put(_recaps, (_writer, _item) => _writer.Put(_item));
        }

        public static void WriteTimeSync(double _time)
        {
            writer.Reset();
            writer.Put(EMessage.TimeSync);
            writer.Put(_time);
            // Checksum
            ulong bytes = Convert.ToUInt64(_time);
            writer.Put(~bytes);
        }

        public static void WriteKillSync(double _time, KillInfo _info)
        {
            writer.Reset();
            writer.Put(EMessage.KillSync);
            writer.Put(_time);
            writer.Put(_info);
        }

        public static void WritePlayerInfo(PlayerInfo _info)
        {
            writer.Reset();
            writer.Put(_info);
        }

        private static void Put(this NetDataWriter _netDataWriter, Enum _value)
        {
            _netDataWriter.Put(Convert.ToByte(_value));
        }

        private static void Put(this NetDataWriter _netDataWriter, in Vector3 _value)
        {
            _netDataWriter.Put(_value.x);
            _netDataWriter.Put(_value.y);
            _netDataWriter.Put(_value.z);
        }

        private static void Put(this NetDataWriter _netDataWriter, in InputStep _value)
        {
            _netDataWriter.Put(_value.dash);
            _netDataWriter.Put(_value.jump);
            _netDataWriter.Put(_value.movementX);
            _netDataWriter.Put(_value.movementZ);
        }

        private static void Put(this NetDataWriter _netDataWriter, in CharacterController _value)
        {
            _netDataWriter.Put(_value.Velocity);
            _netDataWriter.Put(_value.Position);
            _netDataWriter.Put(_value.Height);
            _netDataWriter.Put(_value.dashStamina);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Sight _value)
        {
            _netDataWriter.Put(_value.Turn);
            _netDataWriter.Put(_value.LookUp);
        }

        private static void Put(this NetDataWriter _netDataWriter, in Snapshot _value)
        {
            _netDataWriter.Put(_value.simulation);
            _netDataWriter.Put(_value.sight);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SimulationStepInfo _value)
        {
            _netDataWriter.Put(_value.input);
            _netDataWriter.Put(_value.simulation);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerInfo _value)
        {
            _netDataWriter.Put(_value.name);
            _netDataWriter.PutAsByte(_value.color);
            _netDataWriter.PutAsByte(_value.head);
        }

        private static byte ConvertHealth(int _health)
        {
            return (byte) (Mathf.Clamp(_health, LifeHistory.c_explosionHealth, LifeHistory.c_fullHealth) - LifeHistory.c_explosionHealth);
        }

        private static void Put(this NetDataWriter _netDataWriter, in PlayerRecapInfo _value)
        {
            _netDataWriter.PutAsByte(_value.id);
            _netDataWriter.PutAsByte(_value.kills);
            _netDataWriter.PutAsByte(_value.deaths);
            _netDataWriter.PutAsByte(ConvertHealth(_value.health));
            _netDataWriter.PutAsByte(_value.ping);
        }

        private static void Put(this NetDataWriter _netDataWriter, in DamageInfo _value)
        {
            _netDataWriter.PutAsByte(ConvertHealth(_value.damage));
            _netDataWriter.PutAsByte(ConvertHealth(_value.maxHealth));
            _netDataWriter.PutAsByte(_value.offenderId);
            _netDataWriter.Put(_value.offenseType);
        }

        private static void Put(this NetDataWriter _netDataWriter, in ShotInfo _value)
        {
            _netDataWriter.Put(_value.position);
            _netDataWriter.Put(_value.sight);
            _netDataWriter.Put(_value.isRocket);
        }

        private static void Put(this NetDataWriter _netDataWriter, in SpawnInfo _value)
        {
            _netDataWriter.PutAsByte(_value.spawnPoint);
        }

        private static void Put(this NetDataWriter _netDataWriter, in KazeInfo _value)
        {
            _netDataWriter.Put(_value.position);
        }

        private static void Put<T>(this NetDataWriter _netDataWriter, IEnumerable<T> _value, Action<NetDataWriter, T> _put)
        {
            int countPosition = _netDataWriter.Length;
            _netDataWriter.PutAsByte(0);
            int count = 0;
            foreach (T item in _value)
            {
                count++;
                _put(_netDataWriter, item);
                if (count >= 255)
                {
                    break;
                }
            }
            writer.Data[countPosition] = (byte) count;
        }

        private static void Put(this NetDataWriter _netDataWriter, in KillInfo _value)
        {
            _netDataWriter.PutAsByte(_value.killerId);
            _netDataWriter.PutAsByte(_value.victimId);
            _netDataWriter.Put(_value.offenseType);
            _netDataWriter.PutAsByte(_value.killerKills);
            _netDataWriter.PutAsByte(_value.victimDeaths);
        }

        private static void PutAsByte(this NetDataWriter _netDataWriter, int _value)
        {
            _netDataWriter.Put((byte) Mathf.Clamp(_value, 0, 255));
        }

    }
}