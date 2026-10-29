PRAGMA journal_mode = WAL;

CREATE TABLE IF NOT EXISTS restaurants (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    name        TEXT    NOT NULL UNIQUE COLLATE NOCASE
                        CHECK (length(trim(name)) > 0 AND length(name) <= 100),
    image_file  TEXT    NULL,
    is_active   INTEGER NOT NULL DEFAULT 1 CHECK (is_active IN (0, 1)),
    created_at  TEXT    NOT NULL
);

-- Позиции меню ресторана (подсказки при вводе, с ценой)
CREATE TABLE IF NOT EXISTS restaurant_items (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    restaurant_id INTEGER NOT NULL REFERENCES restaurants(id) ON DELETE CASCADE,
    name          TEXT    NOT NULL COLLATE NOCASE CHECK (length(trim(name)) > 0 AND length(name) <= 100),
    price         REAL    NULL,
    UNIQUE (restaurant_id, name)
);

-- Люди для списка участников
CREATE TABLE IF NOT EXISTS people (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    name       TEXT    NOT NULL UNIQUE COLLATE NOCASE CHECK (length(trim(name)) > 0 AND length(name) <= 60),
    created_at TEXT    NOT NULL
);

-- Приёмы еды за день: одно основное (is_main = 1) и сколько угодно дополнительных с меткой
CREATE TABLE IF NOT EXISTS meals (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    date          TEXT    NOT NULL CHECK (date GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'),
    restaurant_id INTEGER NOT NULL REFERENCES restaurants(id),
    is_main       INTEGER NOT NULL DEFAULT 0 CHECK (is_main IN (0, 1)),
    is_hidden     INTEGER NOT NULL DEFAULT 0 CHECK (is_hidden IN (0, 1)), -- ресторан убран, чек сохранён и скрыт
    label         TEXT    NULL,
    receipt_file  TEXT    NULL,
    total_amount  REAL    NULL,
    created_at    TEXT    NOT NULL,
    updated_at    TEXT    NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_meals_main ON meals(date) WHERE is_main = 1;
CREATE INDEX IF NOT EXISTS ix_meals_date ON meals(date);

CREATE TABLE IF NOT EXISTS meal_participants (
    id         INTEGER PRIMARY KEY AUTOINCREMENT,
    meal_id    INTEGER NOT NULL REFERENCES meals(id) ON DELETE CASCADE,
    person_id  INTEGER NULL REFERENCES people(id) ON DELETE SET NULL,
    name       TEXT    NOT NULL CHECK (length(trim(name)) > 0),
    sort_order INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS ix_meal_participants_meal ON meal_participants(meal_id);

CREATE TABLE IF NOT EXISTS meal_items (
    id             INTEGER PRIMARY KEY AUTOINCREMENT,
    participant_id INTEGER NOT NULL REFERENCES meal_participants(id) ON DELETE CASCADE,
    name           TEXT    NOT NULL CHECK (length(trim(name)) > 0),
    price          REAL    NULL,
    qty            INTEGER NOT NULL DEFAULT 1 CHECK (qty > 0)
);
CREATE INDEX IF NOT EXISTS ix_meal_items_participant ON meal_items(participant_id);

-- Голосование за ресторан на день: одно активное на дату
CREATE TABLE IF NOT EXISTS votes (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    date                 TEXT    NOT NULL CHECK (date GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'),
    status               TEXT    NOT NULL CHECK (status IN ('open', 'finished', 'cancelled')),
    round                INTEGER NOT NULL DEFAULT 1,
    tie_names            TEXT    NULL,
    winner_restaurant_id INTEGER NULL REFERENCES restaurants(id),
    created_by           TEXT    NOT NULL,
    created_at           TEXT    NOT NULL,
    finished_at          TEXT    NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ux_votes_open ON votes(date) WHERE status = 'open';

CREATE TABLE IF NOT EXISTS vote_options (
    id            INTEGER PRIMARY KEY AUTOINCREMENT,
    vote_id       INTEGER NOT NULL REFERENCES votes(id) ON DELETE CASCADE,
    restaurant_id INTEGER NOT NULL REFERENCES restaurants(id),
    added_by      TEXT    NOT NULL,
    UNIQUE (vote_id, restaurant_id)
);

CREATE TABLE IF NOT EXISTS vote_participants (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    vote_id     INTEGER NOT NULL REFERENCES votes(id) ON DELETE CASCADE,
    name        TEXT    NOT NULL COLLATE NOCASE CHECK (length(trim(name)) > 0),
    has_voted   INTEGER NOT NULL DEFAULT 0,
    is_finished INTEGER NOT NULL DEFAULT 0,
    joined_at   TEXT    NOT NULL,
    UNIQUE (vote_id, name)
);

CREATE TABLE IF NOT EXISTS vote_ballots (
    participant_id INTEGER NOT NULL REFERENCES vote_participants(id) ON DELETE CASCADE,
    option_id      INTEGER NOT NULL REFERENCES vote_options(id) ON DELETE CASCADE,
    PRIMARY KEY (participant_id, option_id)
);

-- Общие позиции чека (соус, упаковка) и сборы (доставка, работа сервиса), делятся на всех или на выбранных
CREATE TABLE IF NOT EXISTS meal_shared_items (
    id        INTEGER PRIMARY KEY AUTOINCREMENT,
    meal_id   INTEGER NOT NULL REFERENCES meals(id) ON DELETE CASCADE,
    name      TEXT    NOT NULL CHECK (length(trim(name)) > 0),
    kind      TEXT    NOT NULL DEFAULT 'item' CHECK (kind IN ('item', 'delivery', 'service')),
    price     REAL    NULL,
    qty       INTEGER NOT NULL DEFAULT 1 CHECK (qty > 0),
    split_all INTEGER NOT NULL DEFAULT 1 CHECK (split_all IN (0, 1))
);
CREATE INDEX IF NOT EXISTS ix_meal_shared_items_meal ON meal_shared_items(meal_id);

CREATE TABLE IF NOT EXISTS meal_shared_item_participants (
    shared_item_id INTEGER NOT NULL REFERENCES meal_shared_items(id) ON DELETE CASCADE,
    participant_id INTEGER NOT NULL REFERENCES meal_participants(id) ON DELETE CASCADE,
    PRIMARY KEY (shared_item_id, participant_id)
);
