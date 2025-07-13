using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class WaveUIManager : MonoBehaviourPun
{
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesText;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            // Pede os dados ao MasterClient
            photonView.RPC("RequestWaveData", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestWaveData(PhotonMessageInfo info)
    {
        // Master envia a wave atual e inimigos restantes ao client que pediu
        photonView.RPC("UpdateWaveUI", info.Sender, WaveSpawner.instance.waveNumber, WaveSpawner.instance.GetEnemiesAlive());
    }

    [PunRPC]
    public void UpdateWaveUI(int wave, int enemiesLeft)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;

        if (enemiesText != null)
            enemiesText.text = "Enemies Left: " + enemiesLeft;
    }

    // Chamado pelo WaveSpawner para sincronizar nos clients
    public void SyncUIFromSpawner(int wave, int enemiesLeft)
    {
        photonView.RPC("UpdateWaveUI", RpcTarget.All, wave, enemiesLeft);
    }
}
