/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using Gibbed.MassEffectAndromeda.FileFormats;
using Newtonsoft.Json;

namespace Gibbed.MassEffectAndromeda.SaveFormats.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WorldMapUnknown1
    {
        #region Fields
        private uint _Unknown1;
        private readonly byte[] _Unknown2;
        private readonly byte[] _Unknown3;
        private readonly byte[] _Unknown4;
        private readonly byte[] _Unknown5;
        private uint _Unknown6;
        private uint _Unknown7;
        private byte[] _Unknown8;
        #endregion

        public WorldMapUnknown1()
        {
            this._Unknown2 = new byte[12];
            this._Unknown3 = new byte[12];
            this._Unknown4 = new byte[12];
            this._Unknown5 = new byte[12];
        }

        #region Properties
        [JsonProperty("unknown1")]
        public uint Unknown1
        {
            get { return this._Unknown1; }
            set { this._Unknown1 = value; }
        }

        [JsonProperty("unknown2")]
        public byte[] Unknown2
        {
            get { return this._Unknown2; }
        }

        [JsonProperty("unknown3")]
        public byte[] Unknown3
        {
            get { return this._Unknown3; }
        }

        [JsonProperty("unknown4")]
        public byte[] Unknown4
        {
            get { return this._Unknown4; }
        }

        [JsonProperty("unknown5")]
        public byte[] Unknown5
        {
            get { return this._Unknown5; }
        }

        [JsonProperty("unknown6")]
        public uint Unknown6
        {
            get { return this._Unknown6; }
            set { this._Unknown6 = value; }
        }

        [JsonProperty("unknown7")]
        public uint Unknown7
        {
            get { return this._Unknown7; }
            set { this._Unknown7 = value; }
        }

        [JsonProperty("unknown8")]
        public byte[] Unknown8
        {
            get { return this._Unknown8; }
            set { this._Unknown8 = value; }
        }
        #endregion

        internal void Read(IBitReader reader)
        {
            reader.PushFrameLength(24);

            this._Unknown1 = reader.ReadUInt32();

            // probably a primitive type
            {
                reader.PushFrameLength(24);

                reader.ReadBytes(this._Unknown2);
                reader.ReadBytes(this._Unknown3);
                reader.ReadBytes(this._Unknown4);
                reader.ReadBytes(this._Unknown5);
                this._Unknown6 = reader.ReadUInt32();
                this._Unknown7 = reader.ReadUInt32();

                // probably another primitive type
                {
                    var unknown8Length = reader.ReadUInt32();
                    this._Unknown8 = reader.ReadBytes((int)unknown8Length);
                }

                reader.PopFrameLength();
            }

            reader.PopFrameLength();
        }

        internal void Write(IBitWriter writer)
        {
            writer.PushFrameLength(24);

            writer.WriteUInt32(this._Unknown1);

            // probably a primitive type
            {
                writer.PushFrameLength(24);

                writer.WriteBytes(this._Unknown2);
                writer.WriteBytes(this._Unknown3);
                writer.WriteBytes(this._Unknown4);
                writer.WriteBytes(this._Unknown5);
                writer.WriteUInt32(this._Unknown6);
                writer.WriteUInt32(this._Unknown7);

                // probably another primitive type
                {
                    writer.WriteUInt32((uint)this._Unknown8.Length);
                    writer.WriteBytes(this._Unknown8);
                }

                writer.PopFrameLength();
            }

            writer.PopFrameLength();
        }
    }
}
