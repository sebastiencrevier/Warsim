using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Warsim.Core.Game.Entities;
using Warsim.Core.Helpers.Hash;

namespace Warsim.Core.Game
{
    public class Map
    {
        public Guid Id { get; set; }

        public string OwnerId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPrivate => !string.IsNullOrEmpty(this.Password);

        public string Password { get; set; }

        public IList<Node> SceneObjects { get; set; }

        public bool IsLocked { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public string SerializedSceneObjects
        {
            get
            {
                return JsonConvert.SerializeObject(this.DatabaseMap());
            }
            set
            {
                this.SceneObjects = JsonConvert.DeserializeObject<IList<Node>>(value);
            }
        }

        // DO NOT REMOVE !
        public Map()
        {
        }

        public static Map CreateEmptyMap(string ownerId, string title, string description, string password = "")
        {
            return new Map(ownerId, title, description, password);
        }

        public static Map CreateNewMap(IList<Node> sceneObjects, string ownerId, string title, string description, string password = "")
        {
            var map = new Map(ownerId, title, description, password);

            map.SceneObjects = sceneObjects;

            return map;
        }

        private Map(string ownerId, string title, string description, string password = "")
        {
            this.OwnerId = ownerId;
            this.Title = title;
            this.Description = description;
            this.Password = PasswordHash.CreateHash(password);
            this.SceneObjects = new List<Node>();
            this.Id = Guid.NewGuid();
        }

        public bool ValidatePassword(string password)
        {
            if (this.IsPrivate)
            {
                if (!PasswordHash.ValidatePassword(password, this.Password))
                {
                    return false;
                }
            }

            return true;
        }
    }
}