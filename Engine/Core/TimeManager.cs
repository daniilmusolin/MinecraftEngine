public class TimeManager {
    private float _timeOfDay = 0.25f; // Начинаем с утра (0.0 = полночь, 0.25 = утро, 0.5 = день, 0.75 = вечер)
    private bool _timeCycle = true;
    private float _timeSpeed = 0.001f; // Медленная скорость как в Minecraft (было 0.02f)

    public float TimeOfDay => _timeOfDay;
    public bool TimeCycle => _timeCycle;
    public float TimeSpeed => _timeSpeed;

    public event Action<float>? OnTimeChanged;

    public TimeManager() {
        // Начинаем с утра
        _timeOfDay = 0.25f;
    }

    public void Update(float deltaTime) {
        if (_timeCycle) {
            // Очень медленное изменение времени (1 цикл за ~20 минут реального времени)
            _timeOfDay += deltaTime * _timeSpeed;
            if (_timeOfDay > 1.0f) _timeOfDay = 0.0f;
            OnTimeChanged?.Invoke(_timeOfDay);
        }
    }

    public void ToggleTimeCycle() {
        _timeCycle = !_timeCycle;
    }

    public void SetTime(float time) {
        _timeOfDay = Math.Clamp(time, 0f, 1f);
        OnTimeChanged?.Invoke(_timeOfDay);
    }

    public void SetTimeSpeed(float speed) {
        _timeSpeed = Math.Max(0.0001f, speed);
    }

    public void AddTime(float amount) {
        _timeOfDay += amount;
        if (_timeOfDay > 1.0f) _timeOfDay = 0.0f;
        if (_timeOfDay < 0.0f) _timeOfDay = 1.0f;
        OnTimeChanged?.Invoke(_timeOfDay);
    }

    public (float r, float g, float b) GetSkyColor() {
        // Плавный переход цветов неба в зависимости от времени
        float dayFactor = Math.Abs(_timeOfDay - 0.5f) * 2.0f;

        // Цвета для разных времен суток
        float nightR = 0.02f, nightG = 0.02f, nightB = 0.08f;      // Ночь
        float sunriseR = 1.0f, sunriseG = 0.5f, sunriseB = 0.2f;    // Рассвет
        float dayR = 0.53f, dayG = 0.81f, dayB = 0.92f;            // День
        float sunsetR = 1.0f, sunsetG = 0.4f, sunsetB = 0.1f;      // Закат

        float r, g, b;

        // Определяем время суток
        if (_timeOfDay < 0.2f) {
            // Ночь -> Рассвет
            float t = _timeOfDay / 0.2f;
            r = nightR + (sunriseR - nightR) * t;
            g = nightG + (sunriseG - nightG) * t;
            b = nightB + (sunriseB - nightB) * t;
        } else if (_timeOfDay < 0.3f) {
            // Рассвет -> День
            float t = (_timeOfDay - 0.2f) / 0.1f;
            r = sunriseR + (dayR - sunriseR) * t;
            g = sunriseG + (dayG - sunriseG) * t;
            b = sunriseB + (dayB - sunriseB) * t;
        } else if (_timeOfDay < 0.7f) {
            // День
            r = dayR;
            g = dayG;
            b = dayB;
        } else if (_timeOfDay < 0.8f) {
            // День -> Закат
            float t = (_timeOfDay - 0.7f) / 0.1f;
            r = dayR + (sunsetR - dayR) * t;
            g = dayG + (sunsetG - dayG) * t;
            b = dayB + (sunsetB - dayB) * t;
        } else {
            // Закат -> Ночь
            float t = (_timeOfDay - 0.8f) / 0.2f;
            r = sunsetR + (nightR - sunsetR) * t;
            g = sunsetG + (nightG - sunsetG) * t;
            b = sunsetB + (nightB - sunsetB) * t;
        }

        return (Math.Clamp(r, 0f, 1f), Math.Clamp(g, 0f, 1f), Math.Clamp(b, 0f, 1f));
    }

    public float GetLightLevel() {
        // Освещение в зависимости от времени
        if (_timeOfDay < 0.2f) {
            // Ночь -> Рассвет
            float t = _timeOfDay / 0.2f;
            return 0.1f + t * 0.4f;
        } else if (_timeOfDay < 0.3f) {
            // Рассвет -> День
            float t = (_timeOfDay - 0.2f) / 0.1f;
            return 0.5f + t * 0.3f;
        } else if (_timeOfDay < 0.7f) {
            // День
            return 0.8f;
        } else if (_timeOfDay < 0.8f) {
            // День -> Закат
            float t = (_timeOfDay - 0.7f) / 0.1f;
            return 0.8f - t * 0.3f;
        } else {
            // Закат -> Ночь
            float t = (_timeOfDay - 0.8f) / 0.2f;
            return 0.5f - t * 0.4f;
        }
    }
}