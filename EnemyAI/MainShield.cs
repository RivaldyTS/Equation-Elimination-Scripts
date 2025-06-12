using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MainShield : MonoBehaviour
{
    public GameObject[] miniShields;
    [Header("Main Shield")]
    public GameObject mainShieldObject;
    [Header("UI")]
    public Transform uiCanvas;
    public TextMeshProUGUI textPrefab;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private Vector3 questionOffset = new Vector3(-2, 2, 0);
    [SerializeField] private Vector3 hintOffset = new Vector3(2, 2, 0);
    [SerializeField] private Vector3 answerOffset = new Vector3(0, 0, 0);
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 1f;
    [Header("Text Scaling")]
    [SerializeField] private float minFontSize = 8f;
    [SerializeField] private float maxFontSize = 15f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float fadeDuration = 0.5f;
    [Header("Taunt")]
    public float tauntDuration = 1.5f;
    private string[] taunts = { "Salah lah!", "Math susah ya?", "Fokus dong!", "Coba lagi deh!", "Bukan itu!" };
    [Header("Sounds")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    [Header("Visual Feedback")]
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float flickerDuration = 0.3f;
    [SerializeField] private float revealDelay = 0.5f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float minEmissionIntensity = -0.5f;
    [SerializeField] private float maxEmissionIntensity = 1f;
    [SerializeField] private float miniShieldInteractDelay = 1.5f;
    public GameObject activationEffectPrefab;
    public GameObject breakEffectPrefab;
    [Header("Question Type")]
    [SerializeField] private bool useRatioQuestion = false;
    [SerializeField] private bool useDivisionQuestion = false;
    [SerializeField] private bool useCubePrismQuestion = false;
    private TextMeshProUGUI questionText;
    private TextMeshProUGUI hintText;
    private TextMeshProUGUI[] answerTexts;
    private TextMeshProUGUI tauntText;
    private float correctAnswer;
    private int correctMiniShieldIndex;
    private bool isActive = false;
    private int wrongAttempts = 0;
    private bool isRefreshed = false;
    private bool canInteractWithMiniShields = false;
    private Renderer[] miniShieldRenderers;
    private Collider[] miniShieldColliders;
    private Renderer mainShieldRenderer;
    private Material mainShieldMaterial;
    private Color baseEmissionColor;
    private Coroutine pulseCoroutine;
    private float floatTime = 0f;
    private Target target;

    void Start()
    {
        gameObject.SetActive(false);
        foreach (GameObject miniShield in miniShields)
        {
            miniShield.SetActive(false);
        }
        target = GetComponentInParent<Target>();
        if (target != null)
        {
            target.onEnemyDeath.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning("MainShield: Could not find Target script on this GameObject or its parent.");
        }
        if (uiCanvas != null && textPrefab != null)
        {
            questionText = Instantiate(textPrefab, uiCanvas);
            questionText.gameObject.SetActive(false);
            hintText = Instantiate(textPrefab, uiCanvas);
            hintText.gameObject.SetActive(false);
            hintText.fontSize = 20;
            answerTexts = new TextMeshProUGUI[miniShields.Length];
            for (int i = 0; i < miniShields.Length; i++)
            {
                answerTexts[i] = Instantiate(textPrefab, uiCanvas);
                answerTexts[i].gameObject.SetActive(false);
            }
            tauntText = Instantiate(textPrefab, uiCanvas);
            tauntText.gameObject.SetActive(false);
        }
        miniShieldRenderers = new Renderer[miniShields.Length];
        miniShieldColliders = new Collider[miniShields.Length];
        for (int i = 0; i < miniShields.Length; i++)
        {
            miniShieldRenderers[i] = miniShields[i].GetComponent<Renderer>();
            miniShieldColliders[i] = miniShields[i].GetComponent<Collider>();
            if (miniShieldColliders[i] == null)
            {
                Debug.LogWarning($"MiniShield {i} does not have a Collider component!");
            }
        }
        if (mainShieldObject != null)
        {
            mainShieldRenderer = mainShieldObject.GetComponent<Renderer>();
            if (mainShieldRenderer != null)
            {
                mainShieldMaterial = mainShieldRenderer.material;
                if (mainShieldMaterial.HasProperty("_EmissionColor"))
                {
                    baseEmissionColor = mainShieldMaterial.GetColor("_EmissionColor");
                }
            }
        }
    }

    void OnDestroy()
    {
        if (target != null)
        {
            target.onEnemyDeath.RemoveListener(OnEnemyDeath);
        }
        DestroyUIElements();
    }

    void Update()
    {
        if (isActive)
        {
            FacePlayer();
            UpdateTextPositions();
            UpdateTextSizes();
            floatTime += Time.deltaTime * floatSpeed;
        }
    }

    public void ActivateShield()
    {
        if (!isActive)
        {
            isActive = true;
            gameObject.SetActive(true);
            canInteractWithMiniShields = false;
            foreach (GameObject miniShield in miniShields)
            {
                miniShield.SetActive(true);
                Collider collider = miniShield.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            wrongAttempts = 0;
            isRefreshed = false;
            if (activationEffectPrefab != null && mainShieldObject != null)
            {
                Instantiate(activationEffectPrefab, mainShieldObject.transform.position, Quaternion.identity);
            }
            if (mainShieldMaterial != null && mainShieldMaterial.HasProperty("_EmissionColor"))
            {
                pulseCoroutine = StartCoroutine(PulseEmission());
            }
            StartCoroutine(GenerateQuestionWithDelay());
            StartCoroutine(EnableMiniShieldInteraction());
        }
    }

    private System.Collections.IEnumerator EnableMiniShieldInteraction()
    {
        yield return new WaitForSeconds(miniShieldInteractDelay);
        canInteractWithMiniShields = true;
        foreach (Collider collider in miniShieldColliders)
        {
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    private void OnEnemyDeath()
    {
        if (isActive)
        {
            DeactivateShield();
        }
    }

    System.Collections.IEnumerator GenerateQuestionWithDelay()
    {
        GenerateQuestionInternal();
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
            Color textColor = questionText.color;
            textColor.a = 0f;
            questionText.color = textColor;
        }
        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
            Color hintColor = hintText.color;
            hintColor.a = 0f;
            hintText.color = hintColor;
        }
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            if (questionText != null)
            {
                Color textColor = questionText.color;
                textColor.a = alpha;
                questionText.color = textColor;
            }
            if (hintText != null)
            {
                Color hintColor = hintText.color;
                hintColor.a = alpha;
                hintText.color = hintColor;
            }
            yield return null;
        }
        if (questionText != null)
        {
            Color textColor = questionText.color;
            textColor.a = 1f;
            questionText.color = textColor;
        }
        if (hintText != null)
        {
            Color hintColor = hintText.color;
            hintColor.a = 1f;
            hintText.color = hintColor;
        }
        yield return new WaitForSeconds(revealDelay);
        RevealAnswersAndGlow();
    }

    void GenerateQuestionInternal()
    {
        if (questionText == null || hintText == null || answerTexts == null) return;
        int selectedQuestionTypes = 0;
        if (useRatioQuestion) selectedQuestionTypes++;
        if (useDivisionQuestion) selectedQuestionTypes++;
        if (useCubePrismQuestion) selectedQuestionTypes++;
        if (selectedQuestionTypes != 1)
        {
            int num1 = Random.Range(1, 7);
            int denom1 = Random.Range(2, 7);
            int multiplier = Random.Range(1, 6) * denom1;
            correctAnswer = (float)(num1 * multiplier) / denom1;
            questionText.text =
            $"Perisai musuh hilang {num1}/{denom1} energi per serangan, kamu serang {multiplier / denom1} kali. Berapa totalnya?";
            int resultNum = num1 * multiplier;
            int resultDenom = denom1;
            hintText.text = $"Petunjuk\n{num1}/{denom1} × {multiplier/denom1} = {resultNum}/{resultDenom}\n= ?";
            List<string> uniqueAnswers = new List<string>();
            uniqueAnswers.Add(((int)correctAnswer).ToString());
            correctMiniShieldIndex = Random.Range(0, miniShields.Length);
            while (uniqueAnswers.Count < miniShields.Length)
            {
                int wrongAnswer = (int)correctAnswer + Random.Range(-2, 3);
                if (wrongAnswer <= 0) wrongAnswer = 1;
                string wrongAnswerStr = wrongAnswer.ToString();
                if (!uniqueAnswers.Contains(wrongAnswerStr))
                {
                    uniqueAnswers.Add(wrongAnswerStr);
                }
            }
            for (int i = 0; i < miniShields.Length; i++)
            {
                if (i == correctMiniShieldIndex)
                {
                    answerTexts[i].text = uniqueAnswers[0];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(0.7f, 1f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                else
                {
                    answerTexts[i].text = uniqueAnswers[i < correctMiniShieldIndex ? i + 1 : i];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(1f, 0.7f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                answerTexts[i].gameObject.SetActive(false);
            }
        }
        else if (useRatioQuestion)
        {
            int ratioPart1 = Random.Range(1, 6);
            int ratioPart2 = Random.Range(1, 6);
            int givenQuantity = Random.Range(2, 7) * ratioPart1;
            float groups = (float)givenQuantity / ratioPart1;
            correctAnswer = groups * ratioPart2;
            questionText.text =
            $"Perisai musuh nyala kalau {ratioPart1} lampu merah sama dengan {ratioPart2} lampu biru. Ada {givenQuantity} merah, berapa biru?";
            hintText.text =
            $"Petunjuk\n{ratioPart1} merah = {ratioPart2} biru\nKelompok: {givenQuantity} ÷ {ratioPart1} = {(int)groups}\nBiru: {groups} × {ratioPart2} = ?";
            List<string> uniqueAnswers = new List<string>();
            uniqueAnswers.Add(((int)correctAnswer).ToString());
            correctMiniShieldIndex = Random.Range(0, miniShields.Length);
            while (uniqueAnswers.Count < miniShields.Length)
            {
                int wrongAnswer = (int)correctAnswer + Random.Range(-2, 3);
                if (wrongAnswer <= 0) wrongAnswer = 1;
                string wrongAnswerStr = wrongAnswer.ToString();
                if (!uniqueAnswers.Contains(wrongAnswerStr))
                {
                    uniqueAnswers.Add(wrongAnswerStr);
                }
            }
            for (int i = 0; i < miniShields.Length; i++)
            {
                if (i == correctMiniShieldIndex)
                {
                    answerTexts[i].text = uniqueAnswers[0];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(0.7f, 1f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                else
                {
                    answerTexts[i].text = uniqueAnswers[i < correctMiniShieldIndex ? i + 1 : i];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(1f, 0.7f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                answerTexts[i].gameObject.SetActive(false);
            }
        }
        else if (useDivisionQuestion)
        {
            int num1 = Random.Range(1, 7);
            int denom1 = Random.Range(2, 7);
            int num2 = Random.Range(1, 5);
            int denom2 = Random.Range(2, 6) * num1; // Ensure whole number result
            correctAnswer = (float)(num1 * denom2) / (denom1 * num2);
            questionText.text = $"Perisai musuh punya {num1}/{denom1} energi, dibagi ke {num2}/{denom2/num1} sisi. Berapa energi tiap sisi?";
            int resultNum = num1 * denom2;
            int resultDenom = denom1 * num2;
            hintText.text = $"Petunjuk\n{num1}/{denom1} ÷ {num2}/{denom2/num1} = {num1}/{denom1} × {denom2/num1}/{num2}\n= {resultNum}/{resultDenom}\n= ?";
            List<string> uniqueAnswers = new List<string>();
            uniqueAnswers.Add(((int)correctAnswer).ToString());
            correctMiniShieldIndex = Random.Range(0, miniShields.Length);
            while (uniqueAnswers.Count < miniShields.Length)
            {
                int wrongAnswer = (int)correctAnswer + Random.Range(-2, 3);
                if (wrongAnswer <= 0) wrongAnswer = 1;
                string wrongAnswerStr = wrongAnswer.ToString();
                if (!uniqueAnswers.Contains(wrongAnswerStr))
                {
                    uniqueAnswers.Add(wrongAnswerStr);
                }
            }
            for (int i = 0; i < miniShields.Length; i++)
            {
                if (i == correctMiniShieldIndex)
                {
                    answerTexts[i].text = uniqueAnswers[0];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(0.7f, 1f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                else
                {
                    answerTexts[i].text = uniqueAnswers[i < correctMiniShieldIndex ? i + 1 : i];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(1f, 0.7f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                answerTexts[i].gameObject.SetActive(false);
            }
        }
        else if (useCubePrismQuestion)
        {
            bool isKubus = Random.value > 0.5f;
            if (isKubus)
            {
                int sideLength = Random.Range(2, 9);
                correctAnswer = sideLength * sideLength * sideLength;
                questionText.text = $"Inti perisai musuh kubus, sisi {sideLength} cm. Berapa besar intinya?";
                int step1 = sideLength * sideLength;
                hintText.text = $"Petunjuk\nBesar inti: {sideLength} × {sideLength} = {step1}\n{step1} × {sideLength} = ?";
            }
            else
            {
                int length = Random.Range(2, 9);
                int width = Random.Range(1, 7);
                int height = Random.Range(1, 7);
                correctAnswer = length * width * height;
                questionText.text = $"Inti perisai musuh balok, {length} cm × {width} cm × {height} cm. Berapa besar intinya?";
                int step1 = length * width;
                hintText.text = $"Petunjuk\nBesar inti: {length} × {width} = {step1}\n{step1} × {height} = ?";
            }
            List<string> uniqueAnswers = new List<string>();
            uniqueAnswers.Add(((int)correctAnswer).ToString());
            correctMiniShieldIndex = Random.Range(0, miniShields.Length);
            while (uniqueAnswers.Count < miniShields.Length)
            {
                int wrongAnswer = (int)correctAnswer + Random.Range(-5, 6);
                if (wrongAnswer <= 0) wrongAnswer = 1;
                string wrongAnswerStr = wrongAnswer.ToString();
                if (!uniqueAnswers.Contains(wrongAnswerStr))
                {
                    uniqueAnswers.Add(wrongAnswerStr);
                }
            }
            for (int i = 0; i < miniShields.Length; i++)
            {
                if (i == correctMiniShieldIndex)
                {
                    answerTexts[i].text = uniqueAnswers[0];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(0.7f, 1f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                else
                {
                    answerTexts[i].text = uniqueAnswers[i < correctMiniShieldIndex ? i + 1 : i];
                    if (isRefreshed)
                    {
                        answerTexts[i].color = new Color(1f, 0.7f, 0.7f);
                    }
                    else
                    {
                        answerTexts[i].color = Color.white;
                    }
                }
                answerTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    void RevealAnswersAndGlow()
    {
        for (int i = 0; i < miniShields.Length; i++)
        {
            answerTexts[i].gameObject.SetActive(true);
        }
        if (isRefreshed && miniShieldRenderers[correctMiniShieldIndex] != null)
        {
            StartCoroutine(GlowMiniShield(correctMiniShieldIndex, Color.green));
        }
        UpdateTextPositions();
    }

    void FacePlayer()
    {
        if (Camera.main == null) return;
        Vector3 directionToPlayer = Camera.main.transform.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void UpdateTextPositions()
    {
        if (questionText == null || hintText == null || answerTexts == null || tauntText == null || Camera.main == null) return;
        Vector3 directionToPlayer = (Camera.main.transform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer == Vector3.zero) directionToPlayer = Vector3.forward;
        Vector3 rightVector = Vector3.Cross(Vector3.up, directionToPlayer).normalized;
        float floatOffset = Mathf.Sin(floatTime) * floatAmplitude;
        Vector3 questionBasePos = transform.position + Vector3.up * (questionOffset.y + floatOffset);
        Vector3 questionOffsetWorld = rightVector * questionOffset.x;
        Vector3 questionWorldPos = questionBasePos + questionOffsetWorld;
        Vector3 questionScreenPos = Camera.main.WorldToScreenPoint(questionWorldPos);
        if (questionScreenPos.z < 0) questionScreenPos *= -1;
        questionText.transform.position = Vector3.Lerp(questionText.transform.position, questionScreenPos, smoothSpeed * Time.deltaTime);
        Vector3 hintBasePos = transform.position + Vector3.up * (hintOffset.y + floatOffset);
        Vector3 hintOffsetWorld = rightVector * hintOffset.x;
        Vector3 hintWorldPos = hintBasePos + hintOffsetWorld;
        Vector3 hintScreenPos = Camera.main.WorldToScreenPoint(hintWorldPos);
        if (hintScreenPos.z < 0) hintScreenPos *= -1;
        hintText.transform.position = Vector3.Lerp(hintText.transform.position, hintScreenPos, smoothSpeed * Time.deltaTime);
        for (int i = 0; i < miniShields.Length; i++)
        {
            if (miniShields[i] != null && answerTexts[i] != null)
            {
                Vector3 answerWorldPos = miniShields[i].transform.position + miniShields[i].transform.TransformDirection(answerOffset);
                Vector3 answerScreenPos = Camera.main.WorldToScreenPoint(answerWorldPos);
                if (answerScreenPos.z < 0) answerScreenPos *= -1;
                answerTexts[i].transform.position = Vector3.Lerp(answerTexts[i].transform.position, answerScreenPos, smoothSpeed * Time.deltaTime);
            }
        }
        Vector3 tauntWorldPos = transform.position + new Vector3(0, 1.5f, 0);
        Vector3 tauntScreenPos = Camera.main.WorldToScreenPoint(tauntWorldPos);
        if (tauntScreenPos.z < 0) tauntScreenPos *= -1;
        tauntText.transform.position = Vector3.Lerp(tauntText.transform.position, tauntScreenPos, smoothSpeed * Time.deltaTime);
    }

    void UpdateTextSizes()
    {
        if (Camera.main == null) return;
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        float t = (clampedDistance - minDistance) / (maxDistance - minDistance);
        float fontSize = Mathf.Lerp(maxFontSize, minFontSize, t);
        if (questionText != null)
        {
            questionText.fontSize = fontSize;
        }
        if (hintText != null)
        {
            hintText.fontSize = fontSize * 0.8f;
        }
        if (answerTexts != null)
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] != null)
                {
                    answerTexts[i].fontSize = fontSize * 0.6f;
                }
            }
        }
        if (tauntText != null)
        {
            tauntText.fontSize = fontSize * 0.8f;
        }
    }

    public void CheckMiniShieldHit(GameObject hitMiniShield)
    {
        if (!isActive || !canInteractWithMiniShields) return;
        for (int i = 0; i < miniShields.Length; i++)
        {
            if (hitMiniShield == miniShields[i])
            {
                if (i == correctMiniShieldIndex)
                {
                    if (correctSound != null)
                    {
                        PlayTemporarySound(correctSound);
                    }
                    DeactivateShield();
                }
                else
                {
                    wrongAttempts++;
                    if (wrongSound != null)
                    {
                        PlayTemporarySound(wrongSound);
                    }
                    ShowTaunt();
                    if (miniShieldRenderers[i] != null)
                    {
                        StartCoroutine(FlashMiniShield(i, Color.red));
                    }
                    if (mainShieldRenderer != null)
                    {
                        StartCoroutine(FlickerMainShield());
                    }
                    if (wrongAttempts >= 3)
                    {
                        wrongAttempts = 0;
                        isRefreshed = true;
                        StartCoroutine(GenerateQuestionWithDelay());
                    }
                }
                break;
            }
        }
    }

    void PlayTemporarySound(AudioClip clip)
    {
        GameObject soundObject = new GameObject("TempAudio");
        soundObject.transform.position = transform.position;
        AudioSource tempAudio = soundObject.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.Play();
        Destroy(soundObject, clip.length);
    }

    void ShowTaunt()
    {
        if (tauntText != null)
        {
            tauntText.text = taunts[Random.Range(0, taunts.Length)];
            tauntText.gameObject.SetActive(true);
            Invoke(nameof(HideTaunt), tauntDuration);
        }
    }

    void HideTaunt()
    {
        if (tauntText != null)
        {
            tauntText.gameObject.SetActive(false);
        }
    }

    void DeactivateShield()
    {
        isActive = false;
        canInteractWithMiniShields = false;
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
            if (mainShieldMaterial != null && mainShieldMaterial.HasProperty("_EmissionColor"))
            {
                mainShieldMaterial.SetColor("_EmissionColor", baseEmissionColor);
            }
        }
        if (breakEffectPrefab != null && mainShieldObject != null)
        {
            Instantiate(breakEffectPrefab, mainShieldObject.transform.position, Quaternion.identity);
        }
        gameObject.SetActive(false);
        foreach (GameObject miniShield in miniShields)
        {
            miniShield.SetActive(false);
            Collider collider = miniShield.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        DestroyUIElements();
    }

    private void DestroyUIElements()
    {
        if (questionText != null)
        {
            Destroy(questionText.gameObject);
            questionText = null;
        }
        if (hintText != null)
        {
            Destroy(hintText.gameObject);
            hintText = null;
        }
        if (answerTexts != null)
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] != null)
                {
                    Destroy(answerTexts[i].gameObject);
                    answerTexts[i] = null;
                }
            }
            answerTexts = null;
        }
        if (tauntText != null)
        {
            Destroy(tauntText.gameObject);
            tauntText = null;
        }
    }

    string FractionToString(float value, int originalDenom)
    {
        return ((int)value).ToString(); // Always return whole number as string
    }

    System.Collections.IEnumerator GlowMiniShield(int index, Color glowColor)
    {
        if (miniShieldRenderers[index] == null) yield break;
        Material mat = miniShieldRenderers[index].material;
        Color originalColor = mat.color;
        mat.color = glowColor;
        yield return new WaitForSeconds(glowDuration);
        mat.color = originalColor;
    }

    System.Collections.IEnumerator FlashMiniShield(int index, Color flashColor)
    {
        if (miniShieldRenderers[index] == null) yield break;
        Material mat = miniShieldRenderers[index].material;
        Color originalColor = mat.color;
        mat.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        mat.color = originalColor;
    }

    System.Collections.IEnumerator FlickerMainShield()
    {
        if (mainShieldRenderer == null) yield break;
        for (int i = 0; i < 3; i++)
        {
            mainShieldRenderer.enabled = false;
            yield return new WaitForSeconds(flickerDuration / 6f);
            mainShieldRenderer.enabled = true;
            yield return new WaitForSeconds(flickerDuration / 6f);
        }
    }

    System.Collections.IEnumerator PulseEmission()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float intensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, t);
            Color newEmissionColor = baseEmissionColor * intensity;
            mainShieldMaterial.SetColor("_EmissionColor", newEmissionColor);
            yield return null;
        }
    }
}