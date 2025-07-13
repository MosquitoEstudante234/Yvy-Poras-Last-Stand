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
        // Se não for o Master, espera atualizações do Master.
        if (!PhotonNetwork.IsMasterClient)
        {
            // Solicita os dados atuais da wave
            photonView.RPC("RequestWaveData", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestWaveData(PhotonMessageInfo info)
    {
        // Envia os dados atuais para o player que solicitou
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

    // Esse método pode ser chamado por WaveSpawner via evento para manter todos sincronizados
    public void SyncUIFromSpawner(int wave, int enemiesLeft)
    {
        photonView.RPC("UpdateWaveUI", RpcTarget.All, wave, enemiesLeft);
    }
}
