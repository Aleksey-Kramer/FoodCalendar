namespace FoodCalendar.Services;

/// <summary>Переводы интерфейса: ключ → [ru, en, uz]. Узбекский — латиница нового алфавита 2026 года (ö, ğ, ş, ç вместо o‘, g‘, sh, ch; апостроф только как тутук белгиси).</summary>
public static class Texts
{
    public static readonly Dictionary<string, string[]> Map = new();

    private static void Add(string key, string ru, string en, string uz) => Map[key] = new[] { ru, en, uz };

    static Texts()
    {
        // ---- общее ----
        Add("app.title", "Календарь еды", "Food Calendar", "Ovqat taqvimi");
        Add("nav.calendar", "Календарь", "Calendar", "Taqvim");
        Add("nav.restaurants", "Рестораны", "Restaurants", "Restoranlar");
        Add("nav.people", "Люди", "People", "Odamlar");
        Add("ui.language", "Язык", "Language", "Til");
        Add("ui.theme", "Тема: светлая / тёмная", "Theme: light / dark", "Mavzu: yoruğ / qoronği");
        Add("ui.error", "Произошла непредвиденная ошибка.", "An unexpected error occurred.", "Kutilmagan xatolik yuz berdi.");
        Add("ui.reload", "Перезагрузить", "Reload", "Qayta yuklaş");
        Add("ui.loading", "Загрузка…", "Loading…", "Yuklanmoqda…");
        Add("ui.close", "Закрыть", "Close", "Yopiş");
        Add("ui.cancel", "Отмена", "Cancel", "Bekor qiliş");
        Add("ui.add", "Добавить", "Add", "Qöşiş");
        Add("ui.addPlus", "+ Добавить", "+ Add", "+ Qöşiş");
        Add("ui.save", "Save", "Save", "Saqlaş");
        Map["ui.save"][0] = "Сохранить";
        Add("ui.delete", "Удалить", "Delete", "Öçiriş");
        Add("ui.remove", "Убрать", "Remove", "Olib taşlaş");
        Add("ui.edit", "Изменить", "Edit", "Tahrirlaş");
        Add("ui.yes", "Да", "Yes", "Ha");
        Add("ui.no", "Нет", "No", "Yöq");
        Add("ui.sure", "Точно?", "Sure?", "Aniqmi?");
        Add("ui.name", "Название", "Name", "Nomi");
        Add("ui.price", "Цена", "Price", "Narx");
        Add("ui.amount", "Сумма", "Amount", "Summa");
        Add("ui.qty", "Количество", "Quantity", "Miqdor");
        Add("ui.today", "Сегодня", "Today", "Bugun");
        Add("ui.inactive", "неактивен", "inactive", "nofaol");

        // ---- даты ----
        Add("months", "Январь|Февраль|Март|Апрель|Май|Июнь|Июль|Август|Сентябрь|Октябрь|Ноябрь|Декабрь",
            "January|February|March|April|May|June|July|August|September|October|November|December",
            "Yanvar|Fevral|Mart|Aprel|May|Iyun|Iyul|Avgust|Sentabr|Oktabr|Noyabr|Dekabr");
        Add("months.genitive", "января|февраля|марта|апреля|мая|июня|июля|августа|сентября|октября|ноября|декабря",
            "January|February|March|April|May|June|July|August|September|October|November|December",
            "yanvar|fevral|mart|aprel|may|iyun|iyul|avgust|sentabr|oktabr|noyabr|dekabr");
        Add("weekdays", "воскресенье|понедельник|вторник|среда|четверг|пятница|суббота",
            "Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday",
            "yakşanba|duşanba|seşanba|çorşanba|payşanba|juma|şanba");
        Add("weekdays.short", "Пн|Вт|Ср|Чт|Пт|Сб|Вс", "Mon|Tue|Wed|Thu|Fri|Sat|Sun", "Du|Se|Ç|Pa|Ju|Ş|Ya");

        // ---- календарь ----
        Add("cal.prevMonth", "Предыдущий месяц", "Previous month", "Oldingi oy");
        Add("cal.nextMonth", "Следующий месяц", "Next month", "Keyingi oy");
        Add("cal.dayOff", "Выходной", "Day off", "Dam oliş kuni");
        Add("cal.dayOffLower", "выходной", "day off", "dam oliş");
        Add("cal.extras", "Дополнительные приёмы", "Extra meals", "Qöşimça ovqatlar");
        Add("cal.hasReceipt", "Есть чек", "Receipt attached", "Çek bor");
        Add("cal.noReceipt", "Чека нет", "No receipt", "Çek yöq");
        Add("cal.main", "Основное:", "Main:", "Asosiy:");
        Add("cal.deactivated", " (ресторан деактивирован)", " (restaurant deactivated)", " (restoran öçirilgan)");
        Add("cal.pickMain", "Основное блюдо не выбрано. Выберите ресторан:", "No main dish yet. Pick a restaurant:", "Asosiy taom tanlanmagan. Restoran tanlang:");
        Add("cal.noActive", "Нет активных ресторанов.", "No active restaurants.", "Faol restoranlar yöq.");
        Add("cal.addRestaurant", "Добавить ресторан", "Add a restaurant", "Restoran qöşiş");
        Add("cal.details", "Детали дня: чек, участники, дополнительно →", "Day details: receipt, participants, extras →", "Kun tafsilotlari: çek, iştirokçilar, qöşimça →");
        Add("cal.clear", "Очистить", "Clear", "Tozalaş");

        // ---- страница дня ----
        Add("day.title", "День", "Day", "Kun");
        Add("day.badDate", "Неверная дата.", "Invalid date.", "Notöğri sana.");
        Add("day.toCalendar", "К календарю", "Back to calendar", "Taqvimga");
        Add("day.back", "← Календарь", "← Calendar", "← Taqvim");
        Add("day.prev", "Предыдущий день", "Previous day", "Oldingi kun");
        Add("day.next", "Следующий день", "Next day", "Keyingi kun");
        Add("day.weekend", "Выходной: на этот день еду не заказываем.", "Day off: no food orders on this day.", "Dam oliş kuni: bu kunga ovqat buyurtma qilinmaydi.");
        Add("day.mainDish", "Основное блюдо", "Main dish", "Asosiy taom");
        Add("day.pickRestaurant", "Выбрать ресторан", "Pick a restaurant", "Restoran tanlaş");
        Add("day.extras", "Дополнительно", "Extras", "Qöşimça");
        Add("day.extrasHint", "завтрак, перекус, кофе…", "breakfast, snack, coffee…", "nonuşta, gazak, qahva…");
        Add("day.labelPlaceholder", "Метка: завтрак, перекус…", "Label: breakfast, snack…", "Belgi: nonuşta, gazak…");
        Add("day.restaurantPlaceholder", "Ресторан…", "Restaurant…", "Restoran…");
        Add("day.noExtras", "Дополнительных приёмов нет.", "No extra meals.", "Qöşimça ovqatlar yöq.");
        Add("day.changeRestaurant", "Сменить ресторан", "Change restaurant", "Restoranni almaştiriş");
        Add("day.changeRestaurantFor", "Сменить ресторан: {0}", "Change restaurant: {0}", "Restoranni almaştiriş: {0}");
        Add("day.selectRestaurant", "Выберите ресторан.", "Pick a restaurant.", "Restoran tanlang.");
        Add("day.labels", "Завтрак|Перекус|Кофе|Обед|Ужин|Десерт", "Breakfast|Snack|Coffee|Lunch|Dinner|Dessert", "Nonuşta|Gazak|Qahva|Tuşlik|Keçki ovqat|Desert");

        // ---- ошибка ----
        Add("error.title", "Ошибка", "Error", "Xatolik");
        Add("error.text", "При обработке запроса произошла ошибка. Попробуйте обновить страницу.", "Something went wrong while processing the request. Try reloading the page.", "Sörovni bajarişda xatolik yuz berdi. Sahifani yangilab köring.");

        // ---- люди ----
        Add("people.hint", "Список участников для быстрого выбора на странице дня. Удаление из списка не затрагивает прошлые заказы.",
            "People list for quick selection on the day page. Removing someone does not affect past orders.",
            "Kun sahifasida tez tanlaş uçun iştirokçilar röyxati. Röyxatdan öçiriş ötgan buyurtmalarga ta’sir qilmaydi.");
        Add("people.name", "Имя", "Name", "Ism");
        Add("people.empty", "Список пуст.", "The list is empty.", "Röyxat böş.");

        // ---- рестораны ----
        Add("rest.empty", "Ресторанов пока нет.", "No restaurants yet.", "Hozirça restoranlar yöq.");
        Add("rest.menu", "Меню ({0})", "Menu ({0})", "Menyu ({0})");
        Add("rest.deactivate", "Деактивировать", "Deactivate", "Faolsizlantiriş");
        Add("rest.restore", "Восстановить", "Restore", "Tiklaş");
        Add("rest.menuTitle", "Меню: {0}", "Menu: {0}", "Menyu: {0}");
        Add("rest.menuHint", "Позиции подставляются как подсказки при вводе на странице дня. Цена необязательна.", "Items are suggested while typing on the day page. Price is optional.", "Pozitsiyalar kun sahifasida yozişda taklif qilinadi. Narx ixtiyoriy.");
        Add("rest.menuEmpty", "Меню пока пустое.", "The menu is empty.", "Menyu hozirça böş.");
        Add("rest.new", "Новый ресторан", "New restaurant", "Yangi restoran");
        Add("rest.editing", "Редактирование", "Editing", "Tahrirlaş");
        Add("rest.namePlaceholder", "Например, Пицца Неаполь", "e.g. Pizza Napoli", "Masalan, Pizza Napoli");
        Add("rest.image", "Картинка (JPG, PNG, WebP или SVG, до 2 МБ)", "Image (JPG, PNG, WebP or SVG, up to 2 MB)", "Rasm (JPG, PNG, WebP yoki SVG, 2 MB gaça)");
        Add("rest.preview", "Предпросмотр", "Preview", "Oldindan köriş");
        Add("rest.chooseFile", "Выбрать файл", "Choose a file", "Fayl tanlaş");
        Add("rest.dropTitle", "Перетащите картинку сюда", "Drop an image here", "Rasmni şu yerga taşlang");
        Add("rest.dropHint", "или нажмите, чтобы выбрать файл. Из буфера:", "or click to choose a file. From clipboard:", "yoki fayl tanlaş uçun bosing. Buferdan:");
        Add("rest.selectedFile", "Выбран файл: {0} ({1} КБ)", "Selected: {0} ({1} KB)", "Tanlangan fayl: {0} ({1} KB)");
        Add("rest.removeImage", "Убрать текущую картинку", "Remove current image", "Joriy rasmni olib taşlaş");

        // ---- приём еды / чек ----
        Add("meal.removeMainKeep", "Убрать ресторан? Чек сохранится и появится снова при выборе ресторана.", "Remove the restaurant? The receipt is kept and comes back when a restaurant is picked again.", "Restoranni olib taşlaysizmi? Çek saqlanadi va restoran qayta tanlanganda yana körinadi.");
        Add("meal.removeMain", "Убрать ресторан?", "Remove the restaurant?", "Restoranni olib taşlaysizmi?");
        Add("meal.deleteExtra", "Удалить вместе с чеком и участниками?", "Delete together with the receipt and participants?", "Çek va iştirokçilar bilan birga öçirilsinmi?");
        Add("meal.receipt", "Чек", "Receipt", "Çek");
        Add("meal.openPdf", "📄 Открыть PDF", "📄 Open PDF", "📄 PDF ni oçiş");
        Add("meal.openReceipt", "Открыть чек", "Open receipt", "Çekni oçiş");
        Add("meal.removeReceipt", "Убрать чек", "Remove receipt", "Çekni olib taşlaş");
        Add("meal.attachReceipt", "📎 Прикрепить чек", "📎 Attach receipt", "📎 Çek biriktiriş");
        Add("meal.receiptHint", "Картинка или PDF, до 10 МБ", "Image or PDF, up to 10 MB", "Rasm yoki PDF, 10 MB gaça");
        Add("meal.pasteHint", "Перетащите файл сюда или наведите и вставьте из буфера: Ctrl+V", "Drop a file here, or hover and paste from clipboard: Ctrl+V", "Faylni şu yerga taşlang yoki ustiga olib kelib buferdan qöying: Ctrl+V");
        Add("meal.total", "Итого по чеку", "Receipt total", "Çek böyiça jami");
        Add("meal.totalHint", "Считается из позиций участников и общих сборов", "Calculated from participants’ items and shared fees", "Iştirokçilar pozitsiyalari va umumiy yiğimlardan hisoblanadi");
        Add("meal.totalSplit", "личные {0} + общие {1}", "own {0} + shared {1}", "şaxsiy {0} + umumiy {1}");
        Add("meal.participants", "Участники", "Participants", "Iştirokçilar");
        Add("meal.nobody", "Пока никого.", "Nobody yet.", "Hozirça heç kim yöq.");
        Add("meal.ownShared", "своё {0} + общее {1}", "own {0} + shared {1}", "öziniki {0} + umumiy {1}");
        Add("meal.totalWithShare", "Итого с долей общих", "Total incl. share of shared", "Umumiy uluş bilan jami");
        Add("meal.removeParticipant", "Убрать участника", "Remove participant", "Iştirokçini olib taşlaş");
        Add("meal.deleteItem", "Удалить позицию", "Delete item", "Pozitsiyani öçiriş");
        Add("meal.item", "Позиция", "Item", "Pozitsiya");
        Add("meal.saveToMenuHint", "Сохранить позицию и цену в меню ресторана", "Save the item and its price to the restaurant menu", "Pozitsiya va narxni restoran menyusiga saqlaş");
        Add("meal.toMenu", "в меню", "to menu", "menyuga");
        Add("meal.addItem", "+ Позиция", "+ Item", "+ Pozitsiya");
        Add("meal.participantName", "Имя участника", "Participant name", "Iştirokçi ismi");
        Add("meal.saveToList", "сохранить в список", "save to list", "röyxatga saqlaş");
        Add("meal.addParticipant", "+ Участник", "+ Participant", "+ Iştirokçi");
        Add("meal.shared", "Общие позиции и сборы", "Shared items and fees", "Umumiy pozitsiyalar va yiğimlar");
        Add("meal.sharedPlaceholder", "Соус, упаковка, пакет…", "Sauce, packaging, bag…", "Sous, qadoq, paket…");
        Add("meal.splitAll", "на всех", "everyone", "hammaga");
        Add("meal.splitSome", "на выбранных:", "selected only:", "tanlanganlarga:");
        Add("meal.splitAllN", "на всех ({0})", "everyone ({0})", "hammaga ({0})");
        Add("meal.splitNone", "ни на кого (участники удалены)", "nobody (participants removed)", "heç kimga (iştirokçilar öçirilgan)");
        Add("meal.splitFor", "на: {0}", "for: {0}", "kimga: {0}");
        Add("meal.mode", "Формат", "Format", "Format");
        Add("meal.modeDelivery", "Доставка", "Delivery", "Yetkazib beriş");
        Add("meal.modeDineIn", "Еда на месте", "Dine-in", "Joyida ovqatlaniş");
        Add("shared.item", "Общая позиция", "Shared item", "Umumiy pozitsiya");
        Add("shared.delivery", "Доставка", "Delivery", "Yetkazib beriş");
        Add("shared.service", "Работа сервиса", "Service fee", "Xizmat haqi");
        Add("shared.servicecharge", "Обслуживание", "Service charge", "Xizmat körsatiş");
        Add("shared.tips", "Чаевые", "Tips", "Çoyçaqa");
        Add("tip.shared", "· общее", "· shared", "· umumiy");
        Add("receipt.view", "Просмотр", "View", "Köriş");
        Add("receipt.title", "Чек", "Receipt", "Çek");
        Add("receipt.notFound", "Чек не найден.", "Receipt not found.", "Çek topilmadi.");
        Add("receipt.hidden", "Этот ресторан убран из дня: чек скрыт, но сохранён.", "This restaurant was removed from the day: the receipt is hidden but kept.", "Bu restoran kundan olib taşlangan: çek yaşirilgan, lekin saqlangan.");
        Add("receipt.print", "Печать", "Print", "Çop etiş");
        Add("receipt.backToDay", "К дню", "Back to day", "Kunga");
        Add("receipt.own", "Личные позиции", "Own items", "Şaxsiy pozitsiyalar");
        Add("receipt.share", "доля общих", "share of shared", "umumiy uluş");
        Add("receipt.file", "Файл чека", "Receipt file", "Çek fayli");
        Add("receipt.noItems", "Позиций нет", "No items", "Pozitsiyalar yöq");
        Add("receipt.download", "Скачать", "Download", "Yuklab oliş");
        Add("receipt.edit", "Редактировать", "Edit", "Tahrirlaş");
        Add("tip.attached", "🧾 чек прикреплён", "🧾 receipt attached", "🧾 çek biriktirilgan");

        // ---- голосование ----
        Add("vote.title", "Голосование", "Voting", "Ovoz beriş");
        Add("vote.subtitle", "на сегодня или на будущий рабочий день", "for today or a future workday", "bugun yoki kelgusi iş kuni uçun");
        Add("vote.yourName", "Ваше имя", "Your name", "Ismingiz");
        Add("vote.namePlaceholder", "Как вас называть в голосовании", "How to call you in the vote", "Ovoz berişda sizni qanday ataş");
        Add("vote.result", "🏆 {0}: победил {1} (раунд {2}), ресторан добавлен в календарь.", "🏆 {0}: {1} won (round {2}), the restaurant is on the calendar.", "🏆 {0}: {1} ğolib böldi ({2}-raund), restoran taqvimga qöşildi.");
        Add("vote.none", "Активных голосований нет.", "No active votes.", "Faol ovoz berişlar yöq.");
        Add("vote.start", "Начать голосование", "Start a vote", "Ovoz berişni boşlaş");
        Add("vote.whichDay", "На какой день", "Which day", "Qaysi kunga");
        Add("vote.startHint", "Только сегодня или будущие рабочие дни. Отметьте минимум два ресторана и нажмите «Начать» — остальные увидят голосование и присоединятся.",
            "Only today or future workdays. Tick at least two restaurants and press “Start” — others will see the vote and join.",
            "Faqat bugun yoki kelgusi iş kunlari. Kamida ikkita restoranni belgilang va «Boşlaş» ni bosing — boşqalar ovoz berişni körib, qöşiladi.");
        Add("vote.begin", "Начать", "Start", "Boşlaş");
        Add("vote.forDate", "На {0}", "For {0}", "{0} uçun");
        Add("vote.meta", "· раунд {0} · начал(а) {1} · завершили {2} из {3}", "· round {0} · started by {1} · finished {2} of {3}", "· {0}-raund · boşladi: {1} · yakunladi: {3} dan {2}");
        Add("vote.voted", "Проголосовало {0} {1}", "Voted: {0} {1}", "{0} {1} ovoz berdi");
        Add("vote.people", "человек|человека|человек", "person|people", "kişi");
        Add("vote.tie", "Ничья между: {0}. Переголосуйте.", "Tie between: {0}. Vote again.", "Durang: {0}. Qayta ovoz bering.");
        Add("vote.votes", "Голосов", "Votes", "Ovozlar");
        Add("vote.cast", "Голосовать", "Vote", "Ovoz beriş");
        Add("vote.finished", "Вы завершили, ждём остальных", "You’re done, waiting for others", "Siz yakunladingiz, boşqalarni kutamiz");
        Add("vote.finish", "Завершить голосование", "Finish voting", "Ovoz berişni yakunlaş");
        Add("vote.addRestaurant", "Добавить ресторан…", "Add a restaurant…", "Restoran qöşiş…");
        Add("vote.cancelAll", "Отменить голосование для всех?", "Cancel the vote for everyone?", "Ovoz berişni hamma uçun bekor qilasizmi?");
        Add("vote.cancel", "Отменить", "Cancel", "Bekor qiliş");
        Add("vote.pFinished", "Завершил(а)", "Finished", "Yakunladi");
        Add("vote.pVoted", "Проголосовал(а), ещё не завершил(а)", "Voted, not finished yet", "Ovoz berdi, hali yakunlamadi");
        Add("vote.pWaiting", "Ещё не голосовал(а)", "Hasn’t voted yet", "Hali ovoz bermadi");

        // ---- сообщения сервисов ----
        // ---- оплата и администратор ----
        Add("pay.paid", "Оплатил", "Paid", "Töladi");
        Add("pay.pending", "Ожидает оплаты", "Awaiting payment", "Tölov kutilmoqda");
        Add("pay.toggle", "Нажмите, чтобы изменить статус оплаты", "Click to change the payment status", "Tölov holatini özgartiriş uçun bosing");
        Add("auth.login", "Войти", "Log in", "Kiriş");
        Add("auth.logout", "Выйти", "Log out", "Çiqiş");
        Add("auth.admin", "Админ", "Admin", "Admin");
        Add("auth.title", "Вход для администратора", "Administrator login", "Administrator kirişi");
        Add("auth.hint", "Вход нужен только для изменения дней, чеков и оплат. Остальное доступно всем.", "Login is only needed to edit days, receipts and payments. Everything else is open to everyone.", "Kiriş faqat kunlar, çeklar va tölovlarni özgartiriş uçun kerak. Qolgani hammaga oçiq.");
        Add("auth.user", "Логин", "Login", "Login");
        Add("auth.password", "Пароль", "Password", "Parol");
        Add("auth.failed", "Неверный логин или пароль.", "Wrong login or password.", "Login yoki parol notöğri.");
        Add("auth.readOnly", "Изменять дни, чеки и оплаты может только администратор.", "Only the administrator can edit days, receipts and payments.", "Kunlar, çeklar va tölovlarni faqat administrator özgartira oladi.");
        Add("err.adminOnly", "Только администратор может это менять. Войдите.", "Only the administrator can change this. Please log in.", "Buni faqat administrator özgartira oladi. Kiring.");

        Add("err.imageType", "Допустимы только изображения JPEG, PNG, WebP или SVG.", "Only JPEG, PNG, WebP or SVG images are allowed.", "Faqat JPEG, PNG, WebP yoki SVG rasmlar qabul qilinadi.");
        Add("err.fileTooBig2", "Файл больше 2 МБ.", "File is larger than 2 MB.", "Fayl 2 MB dan katta.");
        Add("err.fileEmpty", "Файл пустой.", "File is empty.", "Fayl böş.");
        Add("err.fileEmptyOrBig", "Файл пустой или больше 2 МБ.", "File is empty or larger than 2 MB.", "Fayl böş yoki 2 MB dan katta.");
        Add("err.svgScript", "SVG со скриптами или обработчиками событий не принимается.", "SVG with scripts or event handlers is not accepted.", "Skript yoki hodisa işlovçilari bölgan SVG qabul qilinmaydi.");
        Add("err.amountNegative", "Сумма не может быть отрицательной.", "Amount cannot be negative.", "Summa manfiy bölişi mumkin emas.");
        Add("err.mealNotFound", "Приём еды не найден.", "Meal not found.", "Ovqat topilmadi.");
        Add("err.participantNotFound", "Участник не найден.", "Participant not found.", "Iştirokçi topilmadi.");
        Add("err.sharedKind", "Неизвестный тип общей позиции.", "Unknown shared item type.", "Umumiy pozitsiya turi noma’lum.");
        Add("err.pickParticipants", "Выберите хотя бы одного участника или делите на всех.", "Pick at least one participant or split among everyone.", "Kamida bitta iştirokçini tanlang yoki hammaga böling.");
        Add("err.weekend", "На выходные еду не заказываем: назначение возможно только на будние дни.", "No orders on weekends: only weekdays can be assigned.", "Dam oliş kunlariga ovqat buyurtma qilinmaydi: faqat iş kunlari tayinlanadi.");
        Add("err.restaurantNotFound", "Ресторан не найден.", "Restaurant not found.", "Restoran topilmadi.");
        Add("err.restaurantInactive", "Этот ресторан деактивирован, выберите другой.", "This restaurant is deactivated, pick another one.", "Bu restoran öçirilgan, boşqasini tanlang.");
        Add("err.labelRequired", "Укажите метку: завтрак, перекус, кофе и т.п.", "Enter a label: breakfast, snack, coffee, etc.", "Belgi kiriting: nonuşta, gazak, qahva va h.k.");
        Add("err.labelTooLong", "Метка не должна быть длиннее {0} символов.", "Label cannot be longer than {0} characters.", "Belgi {0} belgidan uzun bölmasligi kerak.");
        Add("err.itemRequired", "Введите название позиции.", "Enter the item name.", "Pozitsiya nomini kiriting.");
        Add("err.nameTooLong", "Название не должно быть длиннее {0} символов.", "Name cannot be longer than {0} characters.", "Nom {0} belgidan uzun bölmasligi kerak.");
        Add("err.priceNegative", "Цена не может быть отрицательной.", "Price cannot be negative.", "Narx manfiy bölişi mumkin emas.");
        Add("err.qtyRange", "Количество от 1 до 99.", "Quantity must be from 1 to 99.", "Miqdor 1 dan 99 gaça.");
        Add("err.personRequired", "Введите имя.", "Enter a name.", "Ism kiriting.");
        Add("err.personTooLong", "Имя не должно быть длиннее {0} символов.", "Name cannot be longer than {0} characters.", "Ism {0} belgidan uzun bölmasligi kerak.");
        Add("err.receiptTooBig", "Файл чека больше 10 МБ.", "Receipt file is larger than 10 MB.", "Çek fayli 10 MB dan katta.");
        Add("err.receiptType", "Чек должен быть картинкой (JPEG, PNG, WebP) или PDF.", "Receipt must be an image (JPEG, PNG, WebP) or a PDF.", "Çek rasm (JPEG, PNG, WebP) yoki PDF bölişi kerak.");
        Add("err.restaurantExists", "Ресторан с таким названием уже есть.", "A restaurant with this name already exists.", "Bunday nomli restoran allaqaçon bor.");
        Add("err.menuItemExists", "Позиция с таким названием уже есть в меню.", "An item with this name is already on the menu.", "Bunday nomli pozitsiya menyuda allaqaçon bor.");
        Add("err.restaurantNameRequired", "Введите название ресторана.", "Enter the restaurant name.", "Restoran nomini kiriting.");
        Add("err.votePast", "Голосовать в прошлое нельзя.", "You cannot vote for a past date.", "Ötgan sanaga ovoz berib bölmaydi.");
        Add("err.voteWeekend", "На выходной голосование не нужно.", "No vote is needed for a day off.", "Dam oliş kuniga ovoz beriş şart emas.");
        Add("err.voteTwo", "Выберите хотя бы два ресторана.", "Pick at least two restaurants.", "Kamida ikkita restoran tanlang.");
        Add("err.voteExists", "Голосование на эту дату уже идёт, присоединяйтесь.", "A vote for this date is already open, join it.", "Bu sanaga ovoz beriş allaqaçon ketmoqda, qöşiling.");
        Add("err.voteOptionExists", "Этот ресторан уже есть в списке.", "This restaurant is already on the list.", "Bu restoran röyxatda allaqaçon bor.");
        Add("err.voteOneOption", "Отметьте хотя бы один вариант.", "Tick at least one option.", "Kamida bitta variantni belgilang.");
        Add("err.voteFirst", "Сначала проголосуйте.", "Vote first.", "Avval ovoz bering.");
        Add("err.votePress", "Сначала нажмите «Голосовать».", "Press “Vote” first.", "Avval «Ovoz beriş» ni bosing.");
        Add("err.voteNotFound", "Голосование не найдено.", "Vote not found.", "Ovoz beriş topilmadi.");
        Add("err.voteClosed", "Голосование уже завершено.", "The vote is already finished.", "Ovoz beriş allaqaçon yakunlangan.");
        Add("err.voteRestaurantInactive", "Ресторан «{0}» деактивирован.", "Restaurant “{0}” is deactivated.", "«{0}» restorani öçirilgan.");
    }
}
