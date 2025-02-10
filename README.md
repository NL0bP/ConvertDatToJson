# Convert Dat To Json
### ��� ��� ��������:
1. **����� �����**:
   - ������������ �������� ����� `map_data`, � ������� ��������� ��������� ����� (`doodad_map`, `npc_map` � �.�.).

2. **����������� ����� �����**:
   - ��������� ������� ��� ��������� ����� � `map_data`.

3. **��������� ������**:
   - � ����� `GameData\map_data`, ��� ������ ����� (��������, `doodad_map` ��� `npc_map`) ��������� ��������������� ����� ��� ���������� JSON ������.
   - ��� `.dat` ����� � ����� ��������������, � ������ ����������� � JSON.

4. **���������� JSON**:
   - JSON ����� ����������� � ����� `GameData\map_data\doodad_map` � `GameData\map_data\npc_map`.

---

### ������ ��������� �����:
#### �������� ���������:
```
C:\GameData\map_data\
    doodad_map\
        e_ancient_forest.dat
        another_doodad.dat
    npc_map\
        e_ancient_forest.dat
        another_npc.dat
```

#### ����� ���������:
```
C:\MyApp\
    GameData\
        map_data\
            doodad_map\
                e_ancient_forest.json
                another_doodad.json
            npc_map\
                e_ancient_forest.json
                another_npc.json
```

---

### ����:
- ������ ��������� ������������ ��������� ����� (`doodad_map`, `npc_map`) � ��������� JSON ����� � ��������������� �����.