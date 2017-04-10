using System;
using System.Collections.Generic;
using System.Linq;

using Warsim.Core.Game.Entities;

namespace Warsim.Core.Game
{
    public static class MapObjectsExtensions
    {
        public static void RemoveRobots(this Map map)
        {
            map.SceneObjects.ToList().RemoveAll(x => x.Type == "robot");
        }

        public static void RemoveUserRobot(this Map map, string userId)
        {
            var robot = map.SceneObjects.SingleOrDefault(x => x.Type == "robot" && x.Id == userId);

            if (robot != null)
            {
                map.SceneObjects.Remove(robot);
            }
        }

        public static void DeselectUserObjects(this Map map, string userId)
        {
            foreach (var obj in map.SceneObjects.Where(x => x.LastUserId == userId))
            {
                obj.Selected = false;
            }
        }

        public static void DeselectAllObjects(this Map map)
        {
            foreach (var obj in map.SceneObjects)
            {
                obj.Selected = false;
            }
        }

        public static void ResetStartArrow(this Map map, string userId)
        {
            foreach (var obj in map.SceneObjects.Where(x => x.LastUserId == userId))
            {
                obj.LastUserId = "";

                if (obj.Type == "depart")
                {
                    obj.Deleted = true;
                }
            }
        }

        public static IList<Node> DatabaseMap(this Map map)
        {
            var dbMap = map.SceneObjects.Select(x => new Node(x)).ToList();

            foreach (var obj in dbMap)
            {
                obj.Selected = false;
                obj.LastUserId = "";
            }

            dbMap.RemoveAll(x => x.Type == "robot" || x.Type == "depart" || x.Deleted);

            return dbMap;
        }

        public static GameStatisticsUpdate UpdateScene(this Map map, IList<Node> updatedSceneObjects, string userId, DateTime timestamp)
        {
            var statsUpdate = new GameStatisticsUpdate { UserId = userId };

            foreach (var updatedSceneObject in updatedSceneObjects)
            {
                // Ignore objects without ids
                if (updatedSceneObject.Id == null)
                {
                    continue;
                }

                updatedSceneObject.LastUpdated = timestamp;
                updatedSceneObject.LastUserId = userId;

                var existingObject = map.SceneObjects.SingleOrDefault(x => x.Id == updatedSceneObject.Id && x.Type == updatedSceneObject.Type);

                if (existingObject != null)
                {
                    // If the object exists and is older than the new one, update its state
                    if (existingObject.LastUpdated < timestamp)
                    {
                        var index = map.SceneObjects.IndexOf(existingObject);

                        map.SceneObjects[index] = updatedSceneObject;

                        if (!updatedSceneObject.Selected)
                        {
                            statsUpdate.MapModifiedCount++;
                        }
                    }
                }
                else if (!updatedSceneObject.Deleted)
                {
                    // Add the new object to the scene
                    map.SceneObjects.Add(updatedSceneObject);

                    switch (updatedSceneObject.Type)
                    {
                        case "poteau":
                            statsUpdate.PostAddedCount++;
                            statsUpdate.MapModifiedCount++;
                            break;
                        case "mur":
                            statsUpdate.WallAddedCount++;
                            statsUpdate.MapModifiedCount++;
                            break;
                        case "lignes":
                            statsUpdate.LineAddedCount += updatedSceneObject.Children.Count;
                            statsUpdate.MapModifiedCount += updatedSceneObject.Children.Count;
                            break;
                    }
                }
            }

            return statsUpdate;
        }
    }
}