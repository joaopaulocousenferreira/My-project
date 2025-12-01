using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AuthenticationManager))]
public class AuthenticationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AuthenticationManager authManager = (AuthenticationManager)target;
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Configuração Principal do Template", EditorStyles.boldLabel);

        // 1. O Interruptor Mestre (Modo de Interface)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("uiMode"));

        // 2. Lógica Condicional para Backend (Só aparece se NÃO for apenas Anônimo)
        if (authManager.uiMode != UIMode.AnonymousOnly)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Configuração do Backend (Multiutilizador)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backendMode"));

            // Só mostra campos da API se o modo for Nuvem
            if (authManager.backendMode == BackendMode.CloudAPI)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Preencha os dados da sua API para salvar o progresso na nuvem.", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("apiEndpoint"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("apiKey"));
            }
        }
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Referências da UI (Conexões)", EditorStyles.boldLabel);

        // 3. Lógica para mostrar os campos de UI corretos baseados no modo
        switch (authManager.uiMode)
        {
            case UIMode.AnonymousOnly:
                EditorGUILayout.HelpBox("Modo Anónimo: Mostra apenas o botão 'Iniciar' e usa salvamento local padrão.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("anonymousStartButton"));
                break;

            case UIMode.LoginOnly:
                EditorGUILayout.HelpBox("Modo Login Obrigatório: O jogador deve logar ou criar conta para entrar.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loginPanel"));
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Campos do Painel de Login", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("usernameInput"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("passwordInput"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loginRegisterButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closeLoginPanelButton"));
                
                // Mesmo no modo LoginOnly, precisamos das referências para quando ele logar
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Interface Logado (Dashboard)", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("continueButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showUserInfoButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInInfoPanel"));
                break;

            case UIMode.Hybrid:
                EditorGUILayout.HelpBox("Modo Híbrido: Permite jogar como anónimo ou fazer login para salvar progresso.", MessageType.None);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Referências do Estado Deslogado", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("anonymousStartButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showLoginPanelButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loginPanel"));
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Referências do Painel de Login", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("usernameInput"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("passwordInput"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loginRegisterButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closeLoginPanelButton"));
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Referências do Estado Logado", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("continueButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showUserInfoButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInInfoPanel"));
                
                // --- CAMPOS DO RANKING ---
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Referências do Ranking", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showRankingButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rankingPanel"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closeRankingButton"));
                // -------------------------

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Referências do Painel de Info (Logado)", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("welcomeText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInUserEmblemIcon"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInUserLevelText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInUserXpBar"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loggedInUserXpText"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("logoutButton"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closeInfoPanelButton"));
                break;
        }

        // Aplica quaisquer mudanças que fizemos nos campos
        serializedObject.ApplyModifiedProperties();
    }
}