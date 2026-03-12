# Фаза 18. DI container и composition root

T504 [ ] Написать test `ServiceRegistration_CanResolveGameplayScreen`.
T505 [ ] Подключить `Microsoft.Extensions.DependencyInjection`.
T506 [ ] Создать `AddMatch3Core(...)`.
T507 [ ] Создать `AddMatch3Presentation(...)`.
T508 [ ] Прогнать tests.
T509 [ ] Зарегистрировать config как `Singleton`.
T510 [ ] Зарегистрировать stateless services как `Singleton`.
T511 [ ] Зарегистрировать session state как scoped lifetime.
T512 [ ] Прогнать tests.
T513 [ ] Перевести platform composition root на `IServiceCollection`.
T514 [ ] Создать session scope для игрового матча.
T515 [ ] Подключить пересоздание session scope при старте новой игры.
T516 [ ] Прогнать tests.
T517 [ ] Проверить, что `Game Core` не зависит от DI-контейнера.
T518 [ ] Проверить, что platform projects остаются composition root.
