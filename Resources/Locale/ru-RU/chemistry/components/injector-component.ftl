## UI

injector-volume-transfer-label = Объём: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Режим: [color=white]{$modeString}[/color] ([color=white]{$transferVolume}u[/color])
injector-volume-label = Объём: [color=white]{$currentVolume}/{$totalVolume}u[/color]
    Режим: [color=white]{$modeString}[/color]
injector-toggle-verb-text = Переключить режим инъектора

## Entity

injector-component-inject-mode-name = ввод
injector-component-draw-mode-name = забор
injector-component-dynamic-mode-name = динамический
injector-component-mode-changed-text = Теперь {$mode}
injector-component-transfer-success-message = Вы перемещаете {$amount}u в {THE($target)}.
injector-component-transfer-success-message-self = Вы перемещаете {$amount}u себе.
injector-component-inject-success-message = Вы вводите {$amount}u в {THE($target)}!
injector-component-inject-success-message-self = Вы вводите {$amount}u себе!
injector-component-draw-success-message = Вы набираете {$amount}u из {THE($target)}.
injector-component-draw-success-message-self = Вы набираете {$amount}u у себя.

## Сообщения об ошибках

injector-component-target-already-full-message = {CAPITALIZE(THE($target))} уже заполнен!
injector-component-target-already-full-message-self = Вы уже заполнены!
injector-component-target-is-empty-message = {CAPITALIZE(THE($target))} пуст!
injector-component-target-is-empty-message-self = Вы пусты!
injector-component-cannot-toggle-draw-message = Слишком полно, чтобы забирать!
injector-component-cannot-toggle-inject-message = Нечего вводить!
injector-component-cannot-toggle-dynamic-message = Невозможно переключить динамический режим!
injector-component-empty-message = {CAPITALIZE(THE($injector))} пуст!
injector-component-blocked-user = Защитное снаряжение заблокировало вашу инъекцию!
injector-component-blocked-other = {CAPITALIZE(THE(POSS-ADJ($target)))} броня заблокировала инъекцию {THE($user)}!
injector-component-cannot-transfer-message = Вы не можете переместить ничего в {THE($target)}!
injector-component-cannot-transfer-message-self = Вы не можете переместить ничего себе!
injector-component-cannot-inject-message = Вы не можете ввести ничего в {THE($target)}!
injector-component-cannot-inject-message-self = Вы не можете ввести ничего себе!
injector-component-cannot-draw-message = Вы не можете забрать ничего из {THE($target)}!
injector-component-cannot-draw-message-self = Вы не можете забрать ничего у себя!
injector-component-ignore-mobs = Этот инъектор может взаимодействовать только с контейнерами!

## Сообщения действий с мобами

injector-component-needle-injecting-user = Вы начинаете вводить иглу.
injector-component-needle-injecting-target = {CAPITALIZE(THE($user))} пытается ввести иглу вам!
injector-component-needle-drawing-user = Вы начинаете забирать иглу.
injector-component-needle-drawing-target = {CAPITALIZE(THE($user))} пытается забрать у вас иглу!
injector-component-spray-injecting-user = Вы начинаете готовить распылитель.
injector-component-spray-injecting-target = {CAPITALIZE(THE($user))} пытается установить распылитель на вас!

## Сообщения успеха всплывающего окна цели

injector-component-feel-prick-message = Вы ощущаете маленький укол!