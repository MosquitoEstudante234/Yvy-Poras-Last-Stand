using UnityEngine;
using MOBAGame.Core;
using MOBAGame.Player;

namespace MOBAGame.Minions
{
    /// <summary>
    /// Trigger para detectar alvos próximos ao Minion (Bases, outros Minions, Jogadores)
    /// Deve ser colocado em um GameObject filho do Minion com um Collider trigger
    /// </summary>
    public class MinionTrigger : MonoBehaviour
    {
        private MinionAI minionAI;
        private MinionHealth minionHealth;

        private void Start()
        {
            // Obtém componentes do pai (o Minion)
            minionAI = GetComponentInParent<MinionAI>();
            minionHealth = GetComponentInParent<MinionHealth>();

            if (minionAI == null)
            {
                Debug.LogError("MinionTrigger precisa estar em um filho de um objeto com MinionAI!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (minionAI == null) return;

            // Detecta Base inimiga (prioridade máxima)
            BaseController baseController = other.GetComponent<BaseController>();
            if (baseController != null)
            {
                // Verifica se é base inimiga
                if (baseController.baseTeam != minionHealth.GetTeam())
                {
                    minionAI.OnTargetEnterRange(other.transform);
                    return;
                }
            }

            // Detecta Minion inimigo
            MinionHealth enemyMinion = other.GetComponent<MinionHealth>();
            if (enemyMinion != null)
            {
                // Verifica se é minion inimigo
                if (enemyMinion.GetTeam() != minionHealth.GetTeam() &&
                    enemyMinion.GetTeam() != Team.None)
                {
                    minionAI.OnTargetEnterRange(other.transform);
                    return;
                }
            }

            // OPCIONAL: Detecta jogador inimigo (se quiser que minions ataquem jogadores)
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Verifica se é jogador inimigo
                if (playerHealth.GetTeam() != minionHealth.GetTeam() &&
                    playerHealth.GetTeam() != Team.None)
                {
                    minionAI.OnTargetEnterRange(other.transform);
                    return;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (minionAI == null) return;

            // Notifica a IA que o alvo saiu do alcance
            minionAI.OnTargetExitRange(other.transform);
        }
    }
}