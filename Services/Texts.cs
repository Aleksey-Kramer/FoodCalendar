namespace FoodCalendar.Services;

/// <summary>Переводы интерфейса: ключ → [ru, en, uz]. Узбекский — латиница.</summary>
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
        Add("ui.theme", "Тема: светлая / тёмная", "Theme: light / dark", "Mavzu: yorug‘ / qorong‘i");
        Add("ui.error", "Произошла непредвиденная ошибка.", "An unexpected error occurred.", "Kutilmagan xatolik yuz berdi.");
        Add("ui.reload", "Перезагрузить", "Reload", "Qayta yuklash");
        Add("ui.loading", "Загрузка…", "Loading…", "Yuklanmoqda…");
        Add("ui.close", "Закрыть", "Close", "Yopish");
        Add("ui.cancel", "Отмена", "Cancel", "Bekor qilish");
        Add("ui.add", "Добавить", "Add", "Qo‘shish");
        Add("ui.addPlus", "+ Добавить", "+ Add", "+ Qo‘shish");
        Add("ui.save", "Save", "Save", "Saqlash");
        Map["ui.save"][0] = "Сохранить";
        Add("ui.delete", "Удалить", "Delete", "O‘chirish");
        Add("ui.remove", "Убрать", "Remove", "Olib tashlash");
        Add("ui.edit", "Изменить", "Edit", "Tahrirlash");
        Add("ui.yes", "Да", "Yes", "Ha");
        Add("ui.no", "Нет", "No", "Yo‘q");
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
            "yakshanba|dushanba|seshanba|chorshanba|payshanba|juma|shanba");
        Add("weekdays.short", "Пн|Вт|Ср|Чт|Пт|Сб|Вс", "Mon|Tue|Wed|Thu|Fri|Sat|Sun", "Du|Se|Ch|Pa|Ju|Sh|Ya");

        // ---- календарь ----
        Add("cal.prevMonth", "Предыдущий месяц", "Previous month", "Oldingi oy");
        Add("cal.nextMonth", "Следующий месяц", "Next month", "Keyingi oy");
        Add("cal.dayOff", "Выходной", "Day off", "Dam olish kuni");
        Add("cal.dayOffLower", "выходной", "day off", "dam olish");
        Add("cal.extras", "Дополнительные приёмы", "Extra meals", "Qo‘shimcha ovqatlar");
        Add("cal.hasReceipt", "Есть чек", "Receipt attached", "Chek bor");
        Add("cal.noReceipt", "Чека нет", "No receipt", "Chek yo‘q");
        Add("cal.main", "Основное:", "Main:", "Asosiy:");
        Add("cal.deactivated", " (ресторан деактивирован)", " (restaurant deactivated)", " (restoran o‘chirilgan)");
        Add("cal.pickMain", "Основное блюдо не выбрано. Выберите ресторан:", "No main dish yet. Pick a restaurant:", "Asosiy taom tanlanmagan. Restoran tanlang:");
        Add("cal.noActive", "Нет активных ресторанов.", "No active restaurants.", "Faol restoranlar yo‘q.");
        Add("cal.addRestaurant", "Добавить ресторан", "Add a restaurant", "Restoran qo‘shish");
        Add("cal.details", "Детали дня: чек, участники, дополнительно →", "Day details: receipt, participants, extras →", "Kun tafsilotlari: chek, ishtirokchilar, qo‘shimcha →");
        Add("cal.clear", "Очистить", "Clear", "Tozalash");

        // ---- страница дня ----
        Add("day.title", "День", "Day", "Kun");
        Add("day.badDate", "Неверная дата.", "Invalid date.", "Noto‘g‘ri sana.");
        Add("day.toCalendar", "К календарю", "Back to calendar", "Taqvimga");
        Add("day.back", "← Календарь", "← Calendar", "← Taqvim");
        Add("day.prev", "Предыдущий день", "Previous day", "Oldingi kun");
        Add("day.next", "Следующий день", "Next day", "Keyingi kun");
        Add("day.weekend", "Выходной: на этот день еду не заказываем.", "Day off: no food orders on this day.", "Dam olish kuni: bu kunga ovqat buyurtma qilinmaydi.");
        Add("day.mainDish", "Основное блюдо", "Main dish", "Asosiy taom");
        Add("day.pickRestaurant", "Выбрать ресторан", "Pick a restaurant", "Restoran tanlash");
        Add("day.extras", "Дополнительно", "Extras", "Qo‘shimcha");
        Add("day.extrasHint", "завтрак, перекус, кофе…", "breakfast, snack, coffee…", "nonushta, gazak, qahva…");
        Add("day.labelPlaceholder", "Метка: завтрак, перекус…", "Label: breakfast, snack…", "Belgi: nonushta, gazak…");
        Add("day.restaurantPlaceholder", "Ресторан…", "Restaurant…", "Restoran…");
        Add("day.noExtras", "Дополнительных приёмов нет.", "No extra meals.", "Qo‘shimcha ovqatlar yo‘q.");
        Add("day.changeRestaurant", "Сменить ресторан", "Change restaurant", "Restoranni almashtirish");
        Add("day.changeRestaurantFor", "Сменить ресторан: {0}", "Change restaurant: {0}", "Restoranni almashtirish: {0}");
        Add("day.selectRestaurant", "Выберите ресторан.", "Pick a restaurant.", "Restoran tanlang.");
        Add("day.labels", "Завтрак|Перекус|Кофе|Обед|Ужин|Десерт", "Breakfast|Snack|Coffee|Lunch|Dinner|Dessert", "Nonushta|Gazak|Qahva|Tushlik|Kechki ovqat|Desert");

        // ---- ошибка ----
        Add("error.title", "Ошибка", "Error", "Xatolik");
        Add("error.text", "При обработке запроса произошла ошибка. Попробуйте обновить страницу.", "Something went wrong while processing the request. Try reloading the page.", "So‘rovni bajarishda xatolik yuz berdi. Sahifani yangilab ko‘ring.");

        // ---- люди ----
        Add("people.hint", "Список участников для быстрого выбора на странице дня. Удаление из списка не затрагивает прошлые заказы.",
            "People list for quick selection on the day page. Removing someone does not affect past orders.",
            "Kun sahifasida tez tanlash uchun ishtirokchilar ro‘yxati. Ro‘yxatdan o‘chirish o‘tgan buyurtmalarga ta’sir qilmaydi.");
        Add("people.name", "Имя", "Name", "Ism");
        Add("people.empty", "Список пуст.", "The list is empty.", "Ro‘yxat bo‘sh.");

        // ---- рестораны ----
        Add("rest.empty", "Ресторанов пока нет.", "No restaurants yet.", "Hozircha restoranlar yo‘q.");
        Add("rest.menu", "Меню ({0})", "Menu ({0})", "Menyu ({0})");
        Add("rest.deactivate", "Деактивировать", "Deactivate", "Faolsizlantirish");
        Add("rest.restore", "Восстановить", "Restore", "Tiklash");
        Add("rest.menuTitle", "Меню: {0}", "Menu: {0}", "Menyu: {0}");
        Add("rest.menuHint", "Позиции подставляются как подсказки при вводе на странице дня. Цена необязательна.", "Items are suggested while typing on the day page. Price is optional.", "Pozitsiyalar kun sahifasida yozishda taklif qilinadi. Narx ixtiyoriy.");
        Add("rest.menuEmpty", "Меню пока пустое.", "The menu is empty.", "Menyu hozircha bo‘sh.");
        Add("rest.new", "Новый ресторан", "New restaurant", "Yangi restoran");
        Add("rest.editing", "Редактирование", "Editing", "Tahrirlash");
        Add("rest.namePlaceholder", "Например, Пицца Неаполь", "e.g. Pizza Napoli", "Masalan, Pizza Napoli");
        Add("rest.image", "Картинка (JPG, PNG, WebP или SVG, до 2 МБ)", "Image (JPG, PNG, WebP or SVG, up to 2 MB)", "Rasm (JPG, PNG, WebP yoki SVG, 2 MB gacha)");
        Add("rest.preview", "Предпросмотр", "Preview", "Oldindan ko‘rish");
        Add("rest.chooseFile", "Выбрать файл", "Choose a file", "Fayl tanlash");
        Add("rest.dropTitle", "Перетащите картинку сюда", "Drop an image here", "Rasmni shu yerga tashlang");
        Add("rest.dropHint", "или нажмите, чтобы выбрать файл. Из буфера:", "or click to choose a file. From clipboard:", "yoki fayl tanlash uchun bosing. Buferdan:");
        Add("rest.selectedFile", "Выбран файл: {0} ({1} КБ)", "Selected: {0} ({1} KB)", "Tanlangan fayl: {0} ({1} KB)");
        Add("rest.removeImage", "Убрать текущую картинку", "Remove current image", "Joriy rasmni olib tashlash");

        // ---- приём еды / чек ----
        Add("meal.removeMainKeep", "Убрать ресторан? Чек сохранится и появится снова при выборе ресторана.", "Remove the restaurant? The receipt is kept and comes back when a restaurant is picked again.", "Restoranni olib tashlaysizmi? Chek saqlanadi va restoran qayta tanlanganda yana ko‘rinadi.");
        Add("meal.removeMain", "Убрать ресторан?", "Remove the restaurant?", "Restoranni olib tashlaysizmi?");
        Add("meal.deleteExtra", "Удалить вместе с чеком и участниками?", "Delete together with the receipt and participants?", "Chek va ishtirokchilar bilan birga o‘chirilsinmi?");
        Add("meal.receipt", "Чек", "Receipt", "Chek");
        Add("meal.openPdf", "📄 Открыть PDF", "📄 Open PDF", "📄 PDF ni ochish");
        Add("meal.openReceipt", "Открыть чек", "Open receipt", "Chekni ochish");
        Add("meal.removeReceipt", "Убрать чек", "Remove receipt", "Chekni olib tashlash");
        Add("meal.attachReceipt", "📎 Прикрепить чек", "📎 Attach receipt", "📎 Chek biriktirish");
        Add("meal.receiptHint", "Картинка или PDF, до 10 МБ", "Image or PDF, up to 10 MB", "Rasm yoki PDF, 10 MB gacha");
        Add("meal.pasteHint", "Перетащите файл сюда или наведите и вставьте из буфера: Ctrl+V", "Drop a file here, or hover and paste from clipboard: Ctrl+V", "Faylni shu yerga tashlang yoki ustiga olib kelib buferdan qo‘ying: Ctrl+V");
        Add("meal.total", "Итого по чеку", "Receipt total", "Chek bo‘yicha jami");
        Add("meal.totalHint", "Считается из позиций участников и общих сборов", "Calculated from participants’ items and shared fees", "Ishtirokchilar pozitsiyalari va umumiy yig‘imlardan hisoblanadi");
        Add("meal.totalSplit", "личные {0} + общие {1}", "own {0} + shared {1}", "shaxsiy {0} + umumiy {1}");
        Add("meal.participants", "Участники", "Participants", "Ishtirokchilar");
        Add("meal.nobody", "Пока никого.", "Nobody yet.", "Hozircha hech kim yo‘q.");
        Add("meal.ownShared", "своё {0} + общее {1}", "own {0} + shared {1}", "o‘ziniki {0} + umumiy {1}");
        Add("meal.totalWithShare", "Итого с долей общих", "Total incl. share of shared", "Umumiy ulush bilan jami");
        Add("meal.removeParticipant", "Убрать участника", "Remove participant", "Ishtirokchini olib tashlash");
        Add("meal.deleteItem", "Удалить позицию", "Delete item", "Pozitsiyani o‘chirish");
        Add("meal.item", "Позиция", "Item", "Pozitsiya");
        Add("meal.saveToMenuHint", "Сохранить позицию и цену в меню ресторана", "Save the item and its price to the restaurant menu", "Pozitsiya va narxni restoran menyusiga saqlash");
        Add("meal.toMenu", "в меню", "to menu", "menyuga");
        Add("meal.addItem", "+ Позиция", "+ Item", "+ Pozitsiya");
        Add("meal.participantName", "Имя участника", "Participant name", "Ishtirokchi ismi");
        Add("meal.saveToList", "сохранить в список", "save to list", "ro‘yxatga saqlash");
        Add("meal.addParticipant", "+ Участник", "+ Participant", "+ Ishtirokchi");
        Add("meal.shared", "Общие позиции и сборы", "Shared items and fees", "Umumiy pozitsiyalar va yig‘imlar");
        Add("meal.sharedPlaceholder", "Соус, упаковка, пакет…", "Sauce, packaging, bag…", "Sous, qadoq, paket…");
        Add("meal.splitAll", "на всех", "everyone", "hammaga");
        Add("meal.splitSome", "на выбранных:", "selected only:", "tanlanganlarga:");
        Add("meal.splitAllN", "на всех ({0})", "everyone ({0})", "hammaga ({0})");
        Add("meal.splitNone", "ни на кого (участники удалены)", "nobody (participants removed)", "hech kimga (ishtirokchilar o‘chirilgan)");
        Add("meal.splitFor", "на: {0}", "for: {0}", "kimga: {0}");
        Add("meal.mode", "Формат", "Format", "Format");
        Add("meal.modeDelivery", "Доставка", "Delivery", "Yetkazib berish");
        Add("meal.modeDineIn", "Еда на месте", "Dine-in", "Joyida ovqatlanish");
        Add("shared.item", "Общая позиция", "Shared item", "Umumiy pozitsiya");
        Add("shared.delivery", "Доставка", "Delivery", "Yetkazib berish");
        Add("shared.service", "Работа сервиса", "Service fee", "Xizmat haqi");
        Add("shared.servicecharge", "Обслуживание", "Service charge", "Xizmat ko‘rsatish");
        Add("shared.tips", "Чаевые", "Tips", "Choychaqa");
        Add("tip.shared", "· общее", "· shared", "· umumiy");
        Add("receipt.view", "Просмотр", "View", "Ko‘rish");
        Add("receipt.title", "Чек", "Receipt", "Chek");
        Add("receipt.notFound", "Чек не найден.", "Receipt not found.", "Chek topilmadi.");
        Add("receipt.hidden", "Этот ресторан убран из дня: чек скрыт, но сохранён.", "This restaurant was removed from the day: the receipt is hidden but kept.", "Bu restoran kundan olib tashlangan: chek yashirilgan, lekin saqlangan.");
        Add("receipt.print", "Печать", "Print", "Chop etish");
        Add("receipt.backToDay", "К дню", "Back to day", "Kunga");
        Add("receipt.own", "Личные позиции", "Own items", "Shaxsiy pozitsiyalar");
        Add("receipt.share", "доля общих", "share of shared", "umumiy ulush");
        Add("receipt.file", "Файл чека", "Receipt file", "Chek fayli");
        Add("receipt.noItems", "Позиций нет", "No items", "Pozitsiyalar yo‘q");
        Add("receipt.download", "Скачать", "Download", "Yuklab olish");
        Add("receipt.edit", "Редактировать", "Edit", "Tahrirlash");
        Add("tip.attached", "🧾 чек прикреплён", "🧾 receipt attached", "🧾 chek biriktirilgan");

        // ---- голосование ----
        Add("vote.title", "Голосование", "Voting", "Ovoz berish");
        Add("vote.subtitle", "на сегодня или на будущий рабочий день", "for today or a future workday", "bugun yoki kelgusi ish kuni uchun");
        Add("vote.yourName", "Ваше имя", "Your name", "Ismingiz");
        Add("vote.namePlaceholder", "Как вас называть в голосовании", "How to call you in the vote", "Ovoz berishda sizni qanday atash");
        Add("vote.result", "🏆 {0}: победил {1} (раунд {2}), ресторан добавлен в календарь.", "🏆 {0}: {1} won (round {2}), the restaurant is on the calendar.", "🏆 {0}: {1} g‘olib bo‘ldi ({2}-raund), restoran taqvimga qo‘shildi.");
        Add("vote.none", "Активных голосований нет.", "No active votes.", "Faol ovoz berishlar yo‘q.");
        Add("vote.start", "Начать голосование", "Start a vote", "Ovoz berishni boshlash");
        Add("vote.whichDay", "На какой день", "Which day", "Qaysi kunga");
        Add("vote.startHint", "Только сегодня или будущие рабочие дни. Отметьте минимум два ресторана и нажмите «Начать» — остальные увидят голосование и присоединятся.",
            "Only today or future workdays. Tick at least two restaurants and press “Start” — others will see the vote and join.",
            "Faqat bugun yoki kelgusi ish kunlari. Kamida ikkita restoranni belgilang va «Boshlash» ni bosing — boshqalar ovoz berishni ko‘rib, qo‘shiladi.");
        Add("vote.begin", "Начать", "Start", "Boshlash");
        Add("vote.forDate", "На {0}", "For {0}", "{0} uchun");
        Add("vote.meta", "· раунд {0} · начал(а) {1} · завершили {2} из {3}", "· round {0} · started by {1} · finished {2} of {3}", "· {0}-raund · boshladi: {1} · yakunladi: {3} dan {2}");
        Add("vote.voted", "Проголосовало {0} {1}", "Voted: {0} {1}", "{0} {1} ovoz berdi");
        Add("vote.people", "человек|человека|человек", "person|people", "kishi");
        Add("vote.tie", "Ничья между: {0}. Переголосуйте.", "Tie between: {0}. Vote again.", "Durang: {0}. Qayta ovoz bering.");
        Add("vote.votes", "Голосов", "Votes", "Ovozlar");
        Add("vote.cast", "Голосовать", "Vote", "Ovoz berish");
        Add("vote.finished", "Вы завершили, ждём остальных", "You’re done, waiting for others", "Siz yakunladingiz, boshqalarni kutamiz");
        Add("vote.finish", "Завершить голосование", "Finish voting", "Ovoz berishni yakunlash");
        Add("vote.addRestaurant", "Добавить ресторан…", "Add a restaurant…", "Restoran qo‘shish…");
        Add("vote.cancelAll", "Отменить голосование для всех?", "Cancel the vote for everyone?", "Ovoz berishni hamma uchun bekor qilasizmi?");
        Add("vote.cancel", "Отменить", "Cancel", "Bekor qilish");
        Add("vote.pFinished", "Завершил(а)", "Finished", "Yakunladi");
        Add("vote.pVoted", "Проголосовал(а), ещё не завершил(а)", "Voted, not finished yet", "Ovoz berdi, hali yakunlamadi");
        Add("vote.pWaiting", "Ещё не голосовал(а)", "Hasn’t voted yet", "Hali ovoz bermadi");

        // ---- сообщения сервисов ----
        // ---- оплата и администратор ----
        Add("pay.paid", "Оплатил", "Paid", "To‘ladi");
        Add("pay.pending", "Ожидает оплаты", "Awaiting payment", "To‘lov kutilmoqda");
        Add("pay.toggle", "Нажмите, чтобы изменить статус оплаты", "Click to change the payment status", "To‘lov holatini o‘zgartirish uchun bosing");
        Add("auth.login", "Войти", "Log in", "Kirish");
        Add("auth.logout", "Выйти", "Log out", "Chiqish");
        Add("auth.admin", "Админ", "Admin", "Admin");
        Add("auth.title", "Вход для администратора", "Administrator login", "Administrator kirishi");
        Add("auth.hint", "Вход нужен только для изменения дней, чеков и оплат. Остальное доступно всем.", "Login is only needed to edit days, receipts and payments. Everything else is open to everyone.", "Kirish faqat kunlar, cheklar va to‘lovlarni o‘zgartirish uchun kerak. Qolgani hammaga ochiq.");
        Add("auth.user", "Логин", "Login", "Login");
        Add("auth.password", "Пароль", "Password", "Parol");
        Add("auth.failed", "Неверный логин или пароль.", "Wrong login or password.", "Login yoki parol noto‘g‘ri.");
        Add("auth.readOnly", "Изменять дни, чеки и оплаты может только администратор.", "Only the administrator can edit days, receipts and payments.", "Kunlar, cheklar va to‘lovlarni faqat administrator o‘zgartira oladi.");
        Add("err.adminOnly", "Только администратор может это менять. Войдите.", "Only the administrator can change this. Please log in.", "Buni faqat administrator o‘zgartira oladi. Kiring.");

        Add("err.imageType", "Допустимы только изображения JPEG, PNG, WebP или SVG.", "Only JPEG, PNG, WebP or SVG images are allowed.", "Faqat JPEG, PNG, WebP yoki SVG rasmlar qabul qilinadi.");
        Add("err.fileTooBig2", "Файл больше 2 МБ.", "File is larger than 2 MB.", "Fayl 2 MB dan katta.");
        Add("err.fileEmpty", "Файл пустой.", "File is empty.", "Fayl bo‘sh.");
        Add("err.fileEmptyOrBig", "Файл пустой или больше 2 МБ.", "File is empty or larger than 2 MB.", "Fayl bo‘sh yoki 2 MB dan katta.");
        Add("err.svgScript", "SVG со скриптами или обработчиками событий не принимается.", "SVG with scripts or event handlers is not accepted.", "Skript yoki hodisa ishlovchilari bo‘lgan SVG qabul qilinmaydi.");
        Add("err.amountNegative", "Сумма не может быть отрицательной.", "Amount cannot be negative.", "Summa manfiy bo‘lishi mumkin emas.");
        Add("err.mealNotFound", "Приём еды не найден.", "Meal not found.", "Ovqat topilmadi.");
        Add("err.participantNotFound", "Участник не найден.", "Participant not found.", "Ishtirokchi topilmadi.");
        Add("err.sharedKind", "Неизвестный тип общей позиции.", "Unknown shared item type.", "Umumiy pozitsiya turi noma’lum.");
        Add("err.pickParticipants", "Выберите хотя бы одного участника или делите на всех.", "Pick at least one participant or split among everyone.", "Kamida bitta ishtirokchini tanlang yoki hammaga bo‘ling.");
        Add("err.weekend", "На выходные еду не заказываем: назначение возможно только на будние дни.", "No orders on weekends: only weekdays can be assigned.", "Dam olish kunlariga ovqat buyurtma qilinmaydi: faqat ish kunlari tayinlanadi.");
        Add("err.restaurantNotFound", "Ресторан не найден.", "Restaurant not found.", "Restoran topilmadi.");
        Add("err.restaurantInactive", "Этот ресторан деактивирован, выберите другой.", "This restaurant is deactivated, pick another one.", "Bu restoran o‘chirilgan, boshqasini tanlang.");
        Add("err.labelRequired", "Укажите метку: завтрак, перекус, кофе и т.п.", "Enter a label: breakfast, snack, coffee, etc.", "Belgi kiriting: nonushta, gazak, qahva va h.k.");
        Add("err.labelTooLong", "Метка не должна быть длиннее {0} символов.", "Label cannot be longer than {0} characters.", "Belgi {0} belgidan uzun bo‘lmasligi kerak.");
        Add("err.itemRequired", "Введите название позиции.", "Enter the item name.", "Pozitsiya nomini kiriting.");
        Add("err.nameTooLong", "Название не должно быть длиннее {0} символов.", "Name cannot be longer than {0} characters.", "Nom {0} belgidan uzun bo‘lmasligi kerak.");
        Add("err.priceNegative", "Цена не может быть отрицательной.", "Price cannot be negative.", "Narx manfiy bo‘lishi mumkin emas.");
        Add("err.qtyRange", "Количество от 1 до 99.", "Quantity must be from 1 to 99.", "Miqdor 1 dan 99 gacha.");
        Add("err.personRequired", "Введите имя.", "Enter a name.", "Ism kiriting.");
        Add("err.personTooLong", "Имя не должно быть длиннее {0} символов.", "Name cannot be longer than {0} characters.", "Ism {0} belgidan uzun bo‘lmasligi kerak.");
        Add("err.receiptTooBig", "Файл чека больше 10 МБ.", "Receipt file is larger than 10 MB.", "Chek fayli 10 MB dan katta.");
        Add("err.receiptType", "Чек должен быть картинкой (JPEG, PNG, WebP) или PDF.", "Receipt must be an image (JPEG, PNG, WebP) or a PDF.", "Chek rasm (JPEG, PNG, WebP) yoki PDF bo‘lishi kerak.");
        Add("err.restaurantExists", "Ресторан с таким названием уже есть.", "A restaurant with this name already exists.", "Bunday nomli restoran allaqachon bor.");
        Add("err.menuItemExists", "Позиция с таким названием уже есть в меню.", "An item with this name is already on the menu.", "Bunday nomli pozitsiya menyuda allaqachon bor.");
        Add("err.restaurantNameRequired", "Введите название ресторана.", "Enter the restaurant name.", "Restoran nomini kiriting.");
        Add("err.votePast", "Голосовать в прошлое нельзя.", "You cannot vote for a past date.", "O‘tgan sanaga ovoz berib bo‘lmaydi.");
        Add("err.voteWeekend", "На выходной голосование не нужно.", "No vote is needed for a day off.", "Dam olish kuniga ovoz berish shart emas.");
        Add("err.voteTwo", "Выберите хотя бы два ресторана.", "Pick at least two restaurants.", "Kamida ikkita restoran tanlang.");
        Add("err.voteExists", "Голосование на эту дату уже идёт, присоединяйтесь.", "A vote for this date is already open, join it.", "Bu sanaga ovoz berish allaqachon ketmoqda, qo‘shiling.");
        Add("err.voteOptionExists", "Этот ресторан уже есть в списке.", "This restaurant is already on the list.", "Bu restoran ro‘yxatda allaqachon bor.");
        Add("err.voteOneOption", "Отметьте хотя бы один вариант.", "Tick at least one option.", "Kamida bitta variantni belgilang.");
        Add("err.voteFirst", "Сначала проголосуйте.", "Vote first.", "Avval ovoz bering.");
        Add("err.votePress", "Сначала нажмите «Голосовать».", "Press “Vote” first.", "Avval «Ovoz berish» ni bosing.");
        Add("err.voteNotFound", "Голосование не найдено.", "Vote not found.", "Ovoz berish topilmadi.");
        Add("err.voteClosed", "Голосование уже завершено.", "The vote is already finished.", "Ovoz berish allaqachon yakunlangan.");
        Add("err.voteRestaurantInactive", "Ресторан «{0}» деактивирован.", "Restaurant “{0}” is deactivated.", "«{0}» restorani o‘chirilgan.");
    }
}
