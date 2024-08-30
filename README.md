# Это тестовая работа, сделанная 08-2024 на Unity3D.
### Суммарно потрачено ~20 часов

Так как все актульные наработкии находятся под авторским правом моего работодателя, то весь код писался с нуля, за исключением используемых библиотек, чтобы сократить время и не изобретать местами велосипед.
Местами получилось грубо, т.к. весь код - импровизация в выходной и после рабочего дня.
Во время проектирования "внутри матчевой логики", язык не повернётся назвать это кором, очень часто хотелось любую реализацию ECS. Удержаться полнстью от идей ECS не смог и LevelService в своём абдейте по сути делает матчинг, благо он простой.
В ECS парадигме было проще т.к. всё свелось бы к компонентам Position + Scale, VelocityComponet (dir + speed), AliveComponent (hp), TargetComponent (enemy target), WeaponReload, RectComponent, TeamComponent, HitComponent (damage + destroyOnHit). 
И на их основе более простыми циклами сделать расчёты
Тогда бы:
EnemySystem - Обращается к Position компоненту у Entity забинженой в TargetComponent и обновляет вектор направления в VelocityComponet.
PlayerInputSystem - Читает инпут игроков и обновляет VelocityComponet. Так же проверяет запрос на выстрел из Input и актуализирует перезарядку и прочее. Тут думаю можно в 1 систему запихать. 
CollisionSystem - по сути всё то что делалает LevelService (но с групировкой по TeamComponent).
Но увы, требования были без ECS.


## Основные решения, используемые либы. 
### A.k.a. "Почему и зачем".
Основые связи были забинжены через VConteiner (DI). Его перфоманс и простота подкупили по бенчмаркам сравнительно с зенджектом. 
Точка входа MainLifetime (AppContext - gameObject на сцене)
Условно весь биндинг разделен на 3 группы.
1) Базы данных. Хранят информацию о конфигах и AssetReference, сами базы хранятся в папке Resources/Databases. Они не имею ссылок на сложные управляемые объекты и хранят только примитивные данные, а значит не повлияют на время загрузки и размер основного приложения.
2) Провайдеры данных. Это "всё", что мы можем превратить в управляемый юнити объект (UnityEngine.Object) или просто какая-то модель. Провайдер реализует биндинг данных из конфига в некий TTarget. Так же всякие EnemiesProvider могут работать с пулом и создавать объект. Биндинг осуществлен через сущность FeatureHandler. Который принимает массив id и может "дать в аренду" некие данные. Когда фича не нужна, её можно dispose. Под фичей можно понимать что угодно. Например, персонаж - у него есть в зависимостях оружие. Значит, его можно забиндить. А когда персонаж будет выгружен, мы выгрузим и зависимости.
3) Сервисы. Это некие системы, управляющие разными частями игры.

Общение сервисов было сделано через 2 способа. Сорри за костыль. Не до конца успел придумать более лучший способ, т.к. обе идеи витали давно в голове и хотелось их опробовать, а тут тестовое подвенулось.
1) VitalRouter - общение через комнады, код которых догенеривается через SourceGenerator. Наверное, лучше было бы большую часть общения перенести на него в данном тестовом. Из минусов подмечу, что наличие возможности где угодно создавать подписки на какие угодно каналы допускает некий хаос при масштабировании и возможно хорошо бы создать аттрибуты для классов которые будет маркерами белого списка пространства комманд или иного способа группировки. 
2) R3 - реактивность. Тут прихожу к выводу, что лучше более ограниченно использовать: местами избыточно оставляет много не кодогенерируемых связей между системами, плюс в теории предположу, что VitalRouter даст больший перфоманс.

## От себя:
Добавил мини локальный мультиплеер. Поэтому для второго игрока кнопки:
1) 0 на цифровой клавиатуре - стрелять. 
2) Стрелки - двигаться
3) 1 и 3 на цифровой клавиатуре - менять скил.
4) У первого игрока движение на wasd
5) Для управления юзал InputSystem, самое сложное было подружить с 1 клавиатурой 2 игроков. Оказалась нужно клон InputActionAsset создать, чтобы можно было назначить на один девайс нескольких игроков. 4 часа убил на это.
6) Для локации заюзал TileMap, место спавна игроков реализованно через специальный тайл SpawnPointTile (подглядел идею у семплов M3 юнитёвых)

## Заметки по перфомансу:
1) Уже к финалу поленился с оптимизацией, и расчёт всех колизий занимает O(enemy * players * (projectile + 1)), надеюсь, не сильно критично.
2) Чтобы оптимизировать снаряды и не париться сильно с их логикой движения заюзал ParticleSystem. 
3) Чуть использовал идеи ECS чтобы оптимизировать расчёт коллизий, сделав их расчёт в 3 этапа. OnBeginUpdate() + колизии + OnCompleteUpdate()
3.1) Сначало обновляем позиции всех и у оружия запоминаем все партикли через NativeList. 
3.2) Ищем через всех enemy пересечение rect(ов) и дестроим, если нужно, через RemoveAtSwapBack. Если enemy прошёл через все снаряды, наносим урон. Как оптимизацию которую не успел, можно было бы для всех Particle одним проходом дополнительно находить суммарный Rect и, если юнит не входит в него, то нет смысла бежать по списку из ~20-50 снарядов. Типо culling такой.
3.3) Пушим в партикл системы новый буфер и заканчивает обновление колизий.
4) Все пулы использованы через UnityPoolApi
5) Вьюхи все для удобства убиваю. Возможно лучше было бы через SetActive или alpha 0 у canvasGroup. Но я не сильно полировал перезапукаемость компонентов и технически поддержка через setActive есть у View.
6) Оружие и персонажей специально сделал без пулов. 2 игрока не часто будут умирать. А смена оружия тоже происходит не так часто и это по сути 1 Go с партиклами.

## В заключении что не успел, но хорошо было бы:
1) Валидаторы для инспектора
2) Аттрибуты для преобразования string id => dropdown. Чтобы выбирать из доступных сущностей.
3) Структура ссылка на конфиг с валидациями. EntryLink { string Database, string ID } c dropdown поведением в инспекторе.
4) Минимизировать R3 перенеся на VitalRouter. 
5) Перенести основную работу в сервисы, оставив более чистыми провайдеры (только спавн и подобные ему операции)
6) Снизить влияние контроллеров. Скорее это "View", тогда у нас будет +- честный MVC, где роль контроллера берут на себя сервисы.
7) Думаю хорошо бы было раздеделить конфиг на чисто данные конфига и конфиг Addressables. Тогда созданная модель в DataProvicer<TModel> могла бы содержать ссылки на конфиг. (Мне очень не нравится идея, что финальная модель может, например, знать о AssetReference, поэтому где мог создавал промежуточные классы-модели)

# Задание:
>1) На сцене располагается маг (с здоровьем, защитой и скоростью передвижения)
>2) Маг должен уметь двигаться по сцене, поворачиваться (стрелками на клавиатуре) и уметь выпускать заклинания (кнопка Х)
>3) Заклинания у мага должны быть нескольких видов (с разным внешним видом и уроном)
>4) У мага должна быть возможность смены текущего заклинания (кнопки Q и W)
>---
>1) Монстры должны быть нескольких видов (с разным внешним видом, количеством здоровья, урона, защиты и скоростью передвижения)
>2) Монстры должны рождаться рандомно за сценой и направляться к магу
>3) На сцене единовременно должно располагаться не более 10 монстров, при смерти одного должен рождаться следующий
>4) При попадании заклинания в монстра его здоровье должно уменьшаться соответственно урону заклинания и защите монстра
>5) При коллизии с магом, его здоровье должно уменьшаться соответственно защите мага и урона от монстра
>
>Размер сцены должен быть ограничен.
По желанию на сцене могут располагаться различные препятствия.
Внешний вид можно обозначить картинкой или цветом.
Расчет урона: здоровье = здоровье урон * защита (0...1).
На графическую составляющую не стоит тратить много времени, она не влияет на оценку решения. Можно использовать готовые ассеты или простые фигуры.
>
>В процессе выполнения тестового задания не забывайте о расширяемости и сложности поддержки. А также о производительности полученного решения. Итоговый результат должен обладать свойствами продакшн кода, насколько это возможно в условиях лимитированного времени. ECS использовать нельзя.
>
>Если не получилось устранить все проблемы из-за нехватки времени, опишите, как от них избавиться, если бы это был продакшн код.
