// Sentetik örnek veri (gerçek oyuncu/kart verisi değildir; CLAUDE.md kod kuralları).

const p1 = '0b7c2d1e-0000-4000-8000-000000000101';
const p2 = '0b7c2d1e-0000-4000-8000-000000000102';
const p3 = '0b7c2d1e-0000-4000-8000-000000000103';

const menuAll = [
  {'screen': 'player.search', 'actions': ['view']},
  {'screen': 'player.locator', 'actions': ['view']},
  {'screen': 'player.history', 'actions': ['view']},
  {'screen': 'live.tables', 'actions': ['view']},
  {'screen': 'live.rating', 'actions': ['view']},
  {'screen': 'slot.floor', 'actions': ['view']},
  {'screen': 'loyalty.points', 'actions': ['view']},
  {'screen': 'loyalty.comp', 'actions': ['request', 'view']},
  {'screen': 'loyalty.host_portfolio', 'actions': ['view']},
  {'screen': 'report.daily', 'actions': ['view']},
  {'screen': 'cage.shift', 'actions': ['open', 'view']},
];

const players = [
  {
    'playerId': p1,
    'playerNumber': 'P-004127',
    'firstName': 'Deniz',
    'lastName': 'Örnekoğlu',
    'activeCardNumber': '9000 0412 7001',
    'isRestricted': false,
  },
  {
    'playerId': p2,
    'playerNumber': 'P-004188',
    'firstName': 'Deniz',
    'lastName': 'Testçi',
    'activeCardNumber': null,
    'isRestricted': false,
  },
  {
    'playerId': p3,
    'playerNumber': 'P-005310',
    'firstName': 'Denizhan',
    'lastName': 'Kurgu',
    'activeCardNumber': '9000 0531 0002',
    'isRestricted': true,
  },
];

const location = {'playerId': p1, 'tableCode': 'BJ-03', 'since': '2026-11-21T17:40:00Z'};

const loyalty = {
  'playerId': p1,
  'balance': 18450,
  'lifetimeEarned': '42310',
  'tier': 'Gold',
  'recentEntries': [
    {'kind': 'Earned', 'points': 620, 'sourceId': '0b7c2d1e-0000-4000-8000-000000000901', 'at': '2026-11-21T16:55:00Z'},
    {'kind': 'Redeemed', 'points': -1500, 'sourceId': '0b7c2d1e-0000-4000-8000-000000000902', 'at': '2026-11-20T21:10:00Z'},
    {'kind': 'Adjusted', 'points': 250, 'sourceId': '0b7c2d1e-0000-4000-8000-000000000903', 'at': '2026-11-19T12:30:00Z'},
    {'kind': 'Earned', 'points': 1180, 'sourceId': '0b7c2d1e-0000-4000-8000-000000000904', 'at': '2026-11-18T23:05:00Z'},
  ],
};

const daily = {
  'date': '2026-11-21',
  'from': '2026-11-20T22:00:00Z',
  'to': '2026-11-21T22:00:00Z',
  'cage': {
    'shiftsOpen': 3,
    'shiftsClosed': 4,
    'reversals': 1,
    'currencies': [
      {'currency': 'TRY', 'chipSales': '1845000.00', 'chipRedemptions': '1312500.50', 'foreignBought': 0, 'foreignSold': 0, 'closingVariance': '-150.00'},
      {'currency': 'EUR', 'chipSales': 64200, 'chipRedemptions': 48950, 'foreignBought': 12000, 'foreignSold': 3500, 'closingVariance': 0},
    ],
  },
  'floor': {
    'ratingsFinished': 57,
    'live': [
      {'currency': 'TRY', 'theoreticalWin': '286400.25', 'casinoResult': '412750.00'},
      {'currency': 'EUR', 'theoreticalWin': 9150, 'casinoResult': '-2300.00'},
    ],
    'slots': [
      {'currency': 'TRY', 'coinIn': '3962100.00', 'machineWin': '318440.75'},
    ],
  },
  'loyalty': {'pointsEarned': 84210, 'pointsRedeemed': 21500},
};

List<Map<String, Object?>> tables() => [
      _table('t1', 'BJ-01', 'Blackjack', 'TRY', 500, 25000, true),
      _table('t2', 'BJ-02', 'Blackjack', 'TRY', 250, 10000, true),
      _table('t3', 'BJ-03', 'Blackjack', 'EUR', 25, 2500, true),
      _table('t4', 'RL-01', 'Roulette', 'TRY', 100, 50000, true),
      _table('t5', 'RL-02', 'Roulette', 'EUR', 10, 5000, false),
      _table('t6', 'PK-01', 'Poker', 'EUR', 50, 5000, false),
      _table('t7', 'BC-01', 'Baccarat', 'GBP', 100, 20000, true),
    ];

Map<String, Object?> _table(String id, String code, String game, String cur, int min, int max, bool open) => {
      'tableId': '0b7c2d1e-0000-4000-8000-0000000002${id.substring(1).padLeft(2, '0')}',
      'code': code,
      'gameType': game,
      'currency': cur,
      'minBet': min,
      'maxBet': max,
      'houseEdgePercent': '1.5',
      'handsPerHour': 60,
      'isOpen': open,
    };

List<Map<String, Object?>> runningRatings() => [
      for (final (i, t) in [('t1', 1), ('t1', 2), ('t3', 3), ('t4', 4), ('t4', 5), ('t4', 6), ('t7', 7)])
        {
          'ratingId': '0b7c2d1e-0000-4000-8000-00000000030$t',
          'playerId': p1,
          'playerNumber': 'P-00412$t',
          'tableId': '0b7c2d1e-0000-4000-8000-0000000002${i.substring(1).padLeft(2, '0')}',
          'tableCode': i,
          'startedAt': '2026-11-21T17:00:00Z',
        },
    ];

List<Map<String, Object?>> slots() => [
      _slot(1, 'A-101', 'Salon A', '2026-11-21T18:02:00Z', '48210.00', '3920.50'),
      _slot(2, 'A-102', 'Salon A', '2026-11-21T18:02:00Z', '31775.00', '-1240.00'),
      _slot(3, 'A-103', 'Salon A', '2026-11-21T17:47:00Z', '22900.00', '2105.25'),
      _slot(4, 'B-201', 'Salon B (VIP)', '2026-11-21T18:01:00Z', '96400.00', '8810.00'),
      _slot(5, 'B-202', 'Salon B (VIP)', null, '0', '0'),
      _slot(6, 'B-203', 'Salon B (VIP)', '2026-11-21T18:00:00Z', '74120.00', '6015.75'),
    ];

Map<String, Object?> _slot(int n, String asset, String area, String? read, String coinIn, String win) => {
      'machineId': '0b7c2d1e-0000-4000-8000-00000000040$n',
      'assetNumber': asset,
      'area': area,
      'currency': 'TRY',
      'lastReadAt': read,
      'todayCoinIn': coinIn,
      'todayWin': win,
    };
