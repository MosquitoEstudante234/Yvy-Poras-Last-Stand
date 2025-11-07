using UnityEngine;
using Photon.Pun;
using MOBAGame.Lobby;
using System.Collections;
using MOBAGame.Core;

namespace MOBAGame.Minions
{
    public class MinionSpawner : MonoBehaviourPun
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject minionPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Team spawnerTeam;
        [SerializeField] private float spawnInterval = 8f;
        [SerializeField] private int maxActiveMinions = 7;

        private int currentActiveMinions = 0;

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SpawnRoutine());
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                if (currentActiveMinions < maxActiveMinions)
                {
                    SpawnMinion();
                }
            }
        }

        private void SpawnMinion()
        {
            GameObject minion = PhotonNetwork.Instantiate(
                minionPrefab.name,
                spawnPoint.position,
                spawnPoint.rotation
            );

            MinionAI minionAI = minion.GetComponent<MinionAI>();
            photonView.RPC("RPC_SetMinionTeam", RpcTarget.All, minion.GetComponent<PhotonView>().ViewID, (int)spawnerTeam);

            currentActiveMinions++;

            // Listener para quando minion morrer
            StartCoroutine(TrackMinionLife(minion));
        }

        private IEnumerator TrackMinionLife(GameObject minion)
        {
            while (minion != null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            currentActiveMinions--;
        }

        [PunRPC]
        private void RPC_SetMinionTeam(int minionViewID, int team)
        {
            PhotonView minionView = PhotonView.Find(minionViewID);
            if (minionView != null)
            {
                MinionAI minionAI = minionView.GetComponent<MinionAI>();
                // Configurar team via reflection ou propriedade pública
            }
        }
    }
}