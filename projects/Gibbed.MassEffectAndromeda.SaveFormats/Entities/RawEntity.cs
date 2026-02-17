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

using System.ComponentModel;
using Newtonsoft.Json;

namespace Gibbed.MassEffectAndromeda.SaveFormats.Entities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RawEntity : INotifyPropertyChanged
    {
        #region Fields
        private uint _Id;
        private int _Data0Length;
        private byte[] _Data0Bytes;
        private int _Data1Length;
        private byte[] _Data1Bytes;
        #endregion

        #region Properties
        [JsonProperty("id")]
        public uint Id
        {
            get { return this._Id; }
            set
            {
                this._Id = value;
                this.NotifyPropertyChanged("Id");
            }
        }

        [JsonProperty("data_0_length")]
        public int Data0Length
        {
            get { return this._Data0Length; }
            set
            {
                this._Data0Length = value;
                this.NotifyPropertyChanged("Data0Length");
            }
        }

        [JsonProperty("data_0_bytes")]
        public byte[] Data0Bytes
        {
            get { return this._Data0Bytes; }
            set
            {
                this._Data0Bytes = value;
                this.NotifyPropertyChanged("Data0Bytes");
            }
        }

        [JsonProperty("data_1_length")]
        public int Data1Length
        {
            get { return this._Data1Length; }
            set
            {
                this._Data1Length = value;
                this.NotifyPropertyChanged("Data1Length");
            }
        }

        [JsonProperty("data_1_bytes")]
        public byte[] Data1Bytes
        {
            get { return this._Data1Bytes; }
            set
            {
                this._Data1Bytes = value;
                this.NotifyPropertyChanged("Data1Bytes");
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
