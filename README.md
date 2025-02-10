# Convert Dat To Json
### Как это работает:
1. **Выбор папки**:
   - Пользователь выбирает папку `map_data`, в которой находятся вложенные папки (`doodad_map`, `npc_map` и т.д.).

2. **Рекурсивный обход папок**:
   - Программа обходит все вложенные папки в `map_data`.

3. **Обработка файлов**:
   - В папке `GameData\map_data`, для каждой папки (например, `doodad_map` или `npc_map`) создается соответствующая папка для сохранения JSON файлов.
   - Все `.dat` файлы в папке обрабатываются, и данные сохраняются в JSON.

4. **Сохранение JSON**:
   - JSON файлы сохраняются в папки `GameData\map_data\doodad_map` и `GameData\map_data\npc_map`.

---

### Пример структуры папок:
#### Исходная структура:
```
C:\GameData\map_data\
    doodad_map\
        e_ancient_forest.dat
        another_doodad.dat
    npc_map\
        e_ancient_forest.dat
        another_npc.dat
```

#### После обработки:
```
C:\MyApp\
    GameData\
        map_data\
            doodad_map\
                doodad_spawns_main_e_ancient_forest.json
                doodad_spawns_main_another_doodad.json
            npc_map\
                npc_spawns_main_e_ancient_forest.json
                npc_spawns_main_another_npc.json
```

---

### Итог:
- Теперь программа обрабатывает вложенные папки (`doodad_map`, `npc_map`) и сохраняет JSON файлы в соответствующие папки.
