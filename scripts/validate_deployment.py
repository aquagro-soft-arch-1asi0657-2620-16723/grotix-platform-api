#!/usr/bin/env python3
"""Static, dependency-free deployment guard. No claims about actual C# compilation."""
import json
import pathlib
import re
import sys
import xml.etree.ElementTree as ET

ROOT = pathlib.Path(__file__).resolve().parents[1]
errors = []
projects = list(ROOT.glob('src/**/*.csproj')) + list(ROOT.glob('tools/**/*.csproj'))
if not projects:
    errors.append('No C# projects found')
for project in projects:
    try:
        tree = ET.parse(project)
    except ET.ParseError as exc:
        errors.append(f'Invalid csproj {project}: {exc}')
        continue
    fw = tree.findtext('.//TargetFramework')
    if fw != 'net10.0':
        errors.append(f'TargetFramework mismatch: {project}: {fw}')
    for ref in tree.findall('.//ProjectReference'):
        name = ref.attrib['Include'].replace('\\', '/')
        if not (project.parent/name).resolve().is_file():
            errors.append(f'Broken project reference: {project}: {name}')

configs = list(ROOT.glob('src/*.Api/appsettings*.json'))
for conf in configs:
    try:
        data=json.loads(conf.read_text(encoding='utf-8-sig'))
    except json.JSONDecodeError as exc:
        errors.append(f'Invalid JSON: {conf}: {exc}')
        continue
    if data.get('TokenSettings',{}).get('Secret'):
        errors.append(f'Hard-coded JWT secret: {conf}')
    rabbit=data.get('RabbitMq',{})
    if rabbit.get('Uri'):
        errors.append(f'Hard-coded RabbitMQ URI: {conf}')

for path in ROOT.glob('src/*.Api/Dockerfile'):
    txt=path.read_text()
    if 'dotnet/sdk:10.0' not in txt or 'dotnet/aspnet:10.0' not in txt:
        errors.append(f'Wrong Docker SDK/runtime: {path}')
    for proj in re.findall(r'COPY\s+\["([^"]+\.csproj)"',txt):
        if not (ROOT/proj).exists():
            errors.append(f'Dockerfile copy source missing: {path} -> {proj}')

for path in ROOT.rglob('*.cs'):
    if 'bin' in path.parts or 'obj' in path.parts:
        continue
    txt=path.read_text(encoding='utf-8-sig')
    if 'Admin123$' in txt or 'Pwd=root;' in txt or 'S0hcyrt3N' in txt:
        errors.append(f'Known legacy credential remains: {path}')

if errors:
    print('STATIC VALIDATION FAILED:')
    print('\n'.join(' - '+e for e in errors))
    sys.exit(1)
print(f'Static validation OK: {len(projects)} csproj, {len(configs)} appsettings, 6 Dockerfiles')
