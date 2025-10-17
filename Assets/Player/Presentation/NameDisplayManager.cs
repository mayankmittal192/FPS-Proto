using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class NameDisplayManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject namePrefab;

    // Map ECS entity to Text GameObject
    private readonly Dictionary<Entity, GameObject> entityToText = new();

    private void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Check if the world still exists
        if (ConnectionManager.clientWorld == null || !ConnectionManager.clientWorld.IsCreated)
            return;

        // Query all NameDisplay entities
        var entityManager = ConnectionManager.clientWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(PlayerGhost));
        var entities = query.ToEntityArray(Allocator.Temp);
        var entitiesNotPresent = entityToText.Keys.ToList();
        var connectionQuery = entityManager.CreateEntityQuery(typeof(NetworkId));
        var localNetworkId = -1;

        if (!connectionQuery.IsEmpty)
        {
            var connectionEntity = connectionQuery.GetSingletonEntity();
            localNetworkId = entityManager.GetComponentData<NetworkId>(connectionEntity).Value;
        }

        foreach (var entity in entities)
        {
            // Do not create name tag for self player (local player)
            var owner = entityManager.GetComponentData<GhostOwner>(entity);
            if (owner.NetworkId == localNetworkId)
                continue;

            var player = entityManager.GetComponentData<PlayerGhost>(entity);

            // Do not show name tag for dead players
            if (player.isDead)
            {
                // Remove name display
                if (entityToText.TryGetValue(entity, out var go))
                {
                    Destroy(go);
                    entityToText.Remove(entity);
                }
                continue;
            }

            // Keep track of which entities are not present this frame
            entitiesNotPresent.Remove(entity);

            // Get or create text object
            if (!entityToText.TryGetValue(entity, out var textObj))
            {
                textObj = Instantiate(namePrefab, transform);
                textObj.name = player.playerName.ToString();
                entityToText[entity] = textObj;

                // Set initial text
                var tmp = textObj.GetComponentInChildren<TextMeshPro>();
                if (tmp != null) tmp.text = player.playerName.ToString();
            }

            // Update position above the player ghost
            var targetTransform = entityManager.GetComponentData<LocalTransform>(entity);
            Vector3 worldPos = targetTransform.Position + new float3(0, 1.5f, 0); // adjust Y offset

            textObj.transform.position = worldPos;
            textObj.transform.LookAt(mainCamera.transform);
        }

        // Clean up the texts for the entities which are not present anymore
        foreach (var entity in entitiesNotPresent)
        {
            if (entityToText.TryGetValue(entity, out var go)) Destroy(go);
            entityToText.Remove(entity);
        }

        entities.Dispose();
    }
}
