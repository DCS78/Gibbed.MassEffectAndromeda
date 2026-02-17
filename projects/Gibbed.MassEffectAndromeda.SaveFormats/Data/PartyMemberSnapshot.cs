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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gibbed.MassEffectAndromeda.FileFormats;
using Newtonsoft.Json;

namespace Gibbed.MassEffectAndromeda.SaveFormats.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PartyMemberSnapshot : INotifyPropertyChanged
    {
        #region Fields
        private int _Level;
        private int _SkillPointCount;
        private int _LowestPurchasableSkillCost;
        private int _SkillPointPerLevel;
        private int _CharacterId;
        private int _MaximumShields;
        private int _MaximumHealth;
        private readonly List<PartyMemberActiveSkill> _ActiveSkills;
        #endregion

        public PartyMemberSnapshot()
        {
            _ActiveSkills = [];
        }

        #region Properties
        [JsonProperty("level")]
        public int Level
        {
            get { return _Level; }
            set
            {
                _Level = value;
                NotifyPropertyChanged(nameof(Level));
            }
        }

        [JsonProperty("skill_point_count")]
        public int SkillPointCount
        {
            get { return _SkillPointCount; }
            set
            {
                _SkillPointCount = value;
                NotifyPropertyChanged(nameof(SkillPointCount));
            }
        }

        [JsonProperty("lowest_purchasable_skill_cost")]
        public int LowestPurchasableSkillCost
        {
            get { return _LowestPurchasableSkillCost; }
            set
            {
                _LowestPurchasableSkillCost = value;
                NotifyPropertyChanged(nameof(LowestPurchasableSkillCost));
            }
        }

        [JsonProperty("skill_point_per_level")]
        public int SkillPointPerLevel
        {
            get { return _SkillPointPerLevel; }
            set
            {
                _SkillPointPerLevel = value;
                NotifyPropertyChanged(nameof(SkillPointPerLevel));
            }
        }

        [JsonProperty("character_id")]
        public int CharacterId
        {
            get { return _CharacterId; }
            set
            {
                _CharacterId = value;
                NotifyPropertyChanged(nameof(CharacterId));
            }
        }

        [JsonProperty("maximum_shields")]
        public int MaximumShields
        {
            get { return _MaximumShields; }
            set
            {
                _MaximumShields = value;
                NotifyPropertyChanged(nameof(MaximumShields));
            }
        }

        [JsonProperty("maximum_health")]
        public int MaximumHealth
        {
            get { return _MaximumHealth; }
            set
            {
                _MaximumHealth = value;
                NotifyPropertyChanged(nameof(MaximumHealth));
            }
        }

        [JsonProperty("active_skills")]
        public List<PartyMemberActiveSkill> ActiveSkills
        {
            get { return _ActiveSkills; }
        }
        #endregion

        internal void Read(IBitReader reader, uint version)
        {
            _ActiveSkills.Clear();
            if (version >= 11)
            {
                reader.PushFrameLength(24);
                _Level = reader.ReadInt32();
                _SkillPointCount = reader.ReadInt32();
                _LowestPurchasableSkillCost = reader.ReadInt32();
                _SkillPointPerLevel = reader.ReadInt32();
                _CharacterId = reader.ReadInt32();
                _MaximumShields = reader.ReadInt32();
                _MaximumHealth = reader.ReadInt32();
                var activeSkillCount = reader.ReadUInt16();
                for (var i = 0; i < activeSkillCount; i++)
                {
                    reader.PushFrameLength(24);
                    var activeSkill = new PartyMemberActiveSkill
                    {
                        LineHash = reader.ReadInt32(),
                        LineRank = reader.ReadInt32(),
                        GroupHash = reader.ReadInt32(),
                        GroupTypeId = reader.ReadInt32()
                    };
                    _ActiveSkills.Add(activeSkill);
                    reader.PopFrameLength();
                }
                reader.PopFrameLength();
            }
            else
            {
                reader.PushFrameLength(24);
                throw new NotImplementedException();
                reader.PopFrameLength();
            }
        }

        internal void Write(IBitWriter writer)
        {
            writer.PushFrameLength(24);
            writer.WriteInt32(_Level);
            writer.WriteInt32(_SkillPointCount);
            writer.WriteInt32(_LowestPurchasableSkillCost);
            writer.WriteInt32(_SkillPointPerLevel);
            writer.WriteInt32(_CharacterId);
            writer.WriteInt32(_MaximumShields);
            writer.WriteInt32(_MaximumHealth);
            writer.WriteUInt16((ushort)_ActiveSkills.Count);
            foreach (PartyMemberActiveSkill activeSkill in _ActiveSkills)
            {
                writer.PushFrameLength(24);
                writer.WriteInt32(activeSkill.LineHash);
                writer.WriteInt32(activeSkill.LineRank);
                writer.WriteInt32(activeSkill.GroupHash);
                writer.WriteInt32(activeSkill.GroupTypeId);
                writer.PopFrameLength();
            }
            writer.PopFrameLength();
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
